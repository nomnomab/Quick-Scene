using System;
using Nomnom.QuickScene.Editor.Utility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public class FloatingHeaderWindow: QuickWindow<FloatingHeaderWindow>, IDisposable {
		private const int MAX_COMPONENTS_PER_ROW = 25;
		
		private static GUIStyle _iconStyle;
		private static GUIStyle _dropdownIconStyle;
		private static GUIStyle _labelIconStyle;
		private static GUIStyle _labelStyle;
		private static Texture _objectIcon;
		private static Texture _dropdownIcon;

		private Vector3 _worldPoint;
		private Vector3 _lastPosCheck;
		private GameObject _sceneObject;
		private Component[] _components;
		private GUIContent[] _componentIcons;
		private ComponentEditorWindow _componentEditorWindow;
		private GameObjectWindow _gameObjectWindow;
		private int _selectedIndex;

		public static FloatingHeaderWindow Open(GameObject obj, Vector2 screenCoords, Vector2 size) {
			Open(screenCoords, size);

			Instance.Init(obj);
			
			return Instance;
		}
		
		public override void Dispose() {
			_sceneObject = null;
			_components = null;
			_componentEditorWindow = null;
			_gameObjectWindow = null;
			_componentIcons = null;
		}

		public override void Init() {
			_labelIconStyle ??= new GUIStyle {
				fixedWidth = 18,
				padding = {
					top = 3,
					left = 2,
					right = 0,
					bottom = 1
				},
				hover = {
					background = Texture2D.grayTexture
				}
			};
			
			_labelStyle ??= new GUIStyle("DD ItemStyle") {
				padding = {
					
				},
				hover = {
					background = Texture2D.grayTexture
				},
				fixedHeight = EditorGUIUtility.singleLineHeight + 9
			};
			
			_iconStyle ??= new GUIStyle {
				fixedWidth = 18,
				padding = {
					top = 2
				},
				hover = {
					background = Texture2D.grayTexture
				}
			};

			_dropdownIconStyle ??= new GUIStyle {
				fixedWidth = 18,
				padding = {
					top = 3,
					left = 1
				},
				hover = {
					background = Texture2D.grayTexture
				}
			};

			if (!_objectIcon) {
				_objectIcon = DataCache.GetIcon("GameObject Icon");
			}
			
			if (!_dropdownIcon) {
				_dropdownIcon = DataCache.GetIcon("CreateAddNew");
			}
		}

		public void Init(GameObject obj) {
			_sceneObject = obj;
			_worldPoint = obj.transform.position;
			_lastPosCheck = _worldPoint;
			_components = obj.GetComponents<Component>();
			_componentIcons = new GUIContent[_components.Length];
			
			UpdateIcons();

			Vector2 size = new Vector2(Instance.CalculateContainerWidth(), EditorGUIUtility.singleLineHeight + 4);
			Rect rect = new Rect(position.position, size);
			
			minSize = maxSize = size;
			position = rect;
		}

		private void UpdateIcons() {
			for (int i = 0; i < _componentIcons.Length; i++) {
				_componentIcons[i] = null;
			}
			
			EditorApplication.update -= DelayIconLoad;
			EditorApplication.update += DelayIconLoad;
		}

		private void DelayIconLoad() {
			if (!OnPreGUI(null)) {
				EditorApplication.update -= DelayIconLoad;
				return;
			}

			if (AssetPreview.IsLoadingAssetPreviews()) {
				return;
			}
			
			// check
			bool canStop = true;
			for (int i = 0; i < _componentIcons.Length; i++) {
				if (_componentIcons[i] != null) {
					continue;
				}
				
				Component component = _components[i];

				if (AssetPreview.IsLoadingAssetPreview(component.GetInstanceID())) {
					canStop = false;
					continue;
				}
				
				Texture texture;
				if (component is MeshRenderer meshRenderer && meshRenderer.sharedMaterial) {
					if (AssetPreview.IsLoadingAssetPreview(meshRenderer.sharedMaterial.GetInstanceID())) {
						canStop = false;
						continue;
					}
					texture = AssetPreview.GetAssetPreview(meshRenderer.sharedMaterial);
				} else {
					texture = EditorGUIUtility.ObjectContent(component, component.GetType()).image;
				}

				if (!texture) {
					canStop = false;
					continue;
				}

				_componentIcons[i] = new GUIContent(texture, component.GetType().Name);
			}

			if (!canStop) {
				return;
			}

			EditorApplication.update -= DelayIconLoad;
		}

		public override void ShowAs(Rect rect, Vector2 size) {
			ShowPopup();
		}

		public override void OnPreRecompile() {
			EditorApplication.update -= DelayIconLoad;
			
			if (_componentEditorWindow) {
				_componentEditorWindow.OnPreRecompile(); // relay
			}

			if (_gameObjectWindow) {
				_gameObjectWindow.OnPreRecompile(); // relay
			}
			
			base.OnPreRecompile();
		}

		public override void OnHierarchyChanged(ref ObjectChangeEventStream stream) {
			if (!OnPreGUI(null)) {
				return;
			}
			
			// validate event
			bool needsRepaint = false;
			for (int i = 0; i < stream.length; i++) {
				ObjectChangeKind e = stream.GetEventType(i);

				switch (e) {
					case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
						stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var data);
						if (data.instanceId != _sceneObject.transform.GetInstanceID()) {
							UpdateIcons();
						}

						break;
					case ObjectChangeKind.DestroyGameObjectHierarchy:
					case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
					case ObjectChangeKind.UpdatePrefabInstances:
					case ObjectChangeKind.ChangeGameObjectStructure:
						needsRepaint = true;
						break;
					default:
						needsRepaint = false;
						break;
				}
			}

			if (!needsRepaint) {
				return;
			}
			
			// ignore position events

			if (_componentEditorWindow) {
				_componentEditorWindow.OnClose();
				_componentEditorWindow = null;
			}

			if (_gameObjectWindow) {
				_gameObjectWindow.OnClose();
				_gameObjectWindow = null;
			}

			if (!_sceneObject) {
				OnClose();
				return;
			}
			
			Init(_sceneObject);
		}

		public override void OnObjectChanged(GameObject oldObj, GameObject newObj) {
			if (_componentEditorWindow) {
				_componentEditorWindow.OnClose(); // relay
			}

			if (_gameObjectWindow) {
				_gameObjectWindow.OnClose(); // relay
			}

			OnClose();
		}

		public override void OnSceneFrame(SceneView sceneView) {
			if (_componentEditorWindow) {
				_componentEditorWindow.OnSceneFrame(sceneView); // relay
			}

			if (_gameObjectWindow) {
				_gameObjectWindow.OnSceneFrame(sceneView); // relay
			}
			
			UpdateScreenCoords(sceneView);
		}

		public override bool OnPreGUI(Event e) {
			if (!_sceneObject) {
				return false;
			}
			
			if (e != null && _sceneObject && e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete) {
				Undo.DestroyObjectImmediate(_sceneObject);
				e.Use();
				return false;
			}
			
			return base.OnPreGUI(e);
		}

		public override void OnDrawGUI(Event e) {
			EditorGUILayout.BeginHorizontal();
			{
				float titleWidth = CalculateTitleWidth();
				
				GUIContent objIcon = new GUIContent(_sceneObject.name, _objectIcon, "GameObject");
				if (GUILayout.Button(GUIContent.none, GUIStyle.none, GUILayout.Width(titleWidth + 20), GUILayout.Height(position.height))) {
					if (e.button == 0) {
						// show a gameobject popup
						if (_componentEditorWindow) {
							CloseComponentWindow();
						}

						if (_gameObjectWindow) {
							CloseGameObjectWindow();
						} else {
							_gameObjectWindow = GameObjectWindow.Open(
								_sceneObject,
								position.position + new Vector2(0, position.height),
								new Vector2(300, 46));
						}
					} else if (e.button == 1) {
						RouterEditorUtility.DisplayObjectContextMenu(
							new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0),
							new Object[]{ _sceneObject }, 
							0);
						e.Use();
					}
				}

				Rect btnRect = GUILayoutUtility.GetLastRect();
				bool isHover = btnRect.Contains(e.mousePosition);
				btnRect.y -= 3;
				btnRect.x += 2;
				if (e.type == EventType.Repaint) {
					_labelStyle.Draw(btnRect, objIcon, isHover, false, false, false);
				}

				// show the first MAX_COMPONENTS_PER_ROW components
				GUILayout.FlexibleSpace();
					
				EditorGUILayout.BeginHorizontal();
				for (int i = 0; i < Mathf.Min(MAX_COMPONENTS_PER_ROW, _components.Length); i++) {
					Component component = _components[i];
					GUIContent content = _componentIcons[i];

					if (GUILayout.Button(content ?? GUIContent.none, _iconStyle, GUILayout.Width(_iconStyle.fixedWidth + 2), GUILayout.Height(position.height))) {
						if (e.button == 0) {
							// show editor in dropdown
							if (_componentEditorWindow) {
								CloseComponentWindow();

								if (_selectedIndex == i) {
									_selectedIndex = -1;
									return;
								}
							}

							if (_gameObjectWindow) {
								CloseGameObjectWindow();
							}

							Vector2 rightUnderBtn = position.position;
							rightUnderBtn += new Vector2(CalculateSpecificComponentPointX(i), position.height);
							_componentEditorWindow = ComponentEditorWindow.Open(component, rightUnderBtn, new Vector2(400, 200));
							_selectedIndex = i;
						} else if (e.button == 1) {
							// show popup menu
							RouterEditorUtility.DisplayObjectContextMenu(
								new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0),
								new Object[]{ component }, 
								0);
							e.Use();
						}
					}
				}

				if (GUILayout.Button(new GUIContent(_dropdownIcon, "Add Component"), _dropdownIconStyle, GUILayout.Width(_iconStyle.fixedWidth + 2), GUILayout.Height(position.size.y))) {
					if (e.button == 0) {
						// show the rest of the components
						Vector2 rightUnderBtn = Vector2.zero;
						// rightUnderBtn += new Vector2(CalculateSpecificComponentPointX(_components.Length), position.height);
						Vector2 size = new Vector2(200, 400);
						Rect rect = new Rect(rightUnderBtn, size);
						rect.y += position.height;
						rect.y -= size.y;
						rect.x += size.x;
						rect.x -= 100;
						RouterEditor.ShowAddComponentWindow(rect, _sceneObject);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndHorizontal();
		}

		public override void OnAfterGUI(Event e) {
			Repaint();
		}

		private void CloseGameObjectWindow() {
			_gameObjectWindow.OnClose();
			_gameObjectWindow = null;
		}

		private void CloseComponentWindow() {
			_componentEditorWindow.OnClose();
			_componentEditorWindow = null;
		}

		private Vector2 CalculateScreenPoint() {
			SceneView lastView = SceneView.lastActiveSceneView;
			// Rect mainWindowRect = RouterEditor.GetEditorMainWindowPos();
			Rect viewRect = lastView.position;
			// viewRect.position -= mainWindowRect.position;
			Vector2 screenCoords = viewRect.position +
			                       HandleUtility.WorldToGUIPoint(_worldPoint) -
			                       position.size * 0.5f;
			
			screenCoords.x = Mathf.Clamp(screenCoords.x, viewRect.min.x, viewRect.max.x - position.width);
			screenCoords.y = Mathf.Clamp(screenCoords.y, viewRect.min.y, viewRect.max.y - position.height - 128);
			
			screenCoords.y += 128;

			return screenCoords;
		}
		private float CalculateTitleWidth() {
			return EditorStyles.label.CalcSize(new GUIContent(_sceneObject.name)).x;
		}

		private float CalculateLeftContainerWidth() {
			return 20 + CalculateTitleWidth();
		}

		private float CalculateContainerWidth() {
			return CalculateLeftContainerWidth() + CalculateComponentWidth();
		}

		private float CalculateComponentWidth() {
			int components = Mathf.Min(MAX_COMPONENTS_PER_ROW, _components.Length + 1);

			return components * (_iconStyle.fixedWidth + 2) + 8;
		}

		private float CalculateSpecificComponentPointX(int componentIndex) {
			float titleWidth = 20 + CalculateTitleWidth();
			titleWidth += componentIndex * _iconStyle.fixedWidth;

			return titleWidth;
		}

		private void UpdateScreenCoords(SceneView sceneView) {
			if (!_sceneObject) {
				return;
			}
			
			Vector2 screenPoint = CalculateScreenPoint();
			Rect pos = position;
			float diff = Vector3.Distance(pos.position, screenPoint);
			float newWidth = CalculateContainerWidth();

			if (Mathf.Abs(pos.size.x - newWidth) >= 3) {
				pos.size = minSize = maxSize = new Vector2(newWidth, pos.height);
				position = pos;
			}
			
			if (diff < 3) {
				return;
			}

			pos.position = screenPoint;
			position = pos;
			sceneView.Repaint();

			if (_componentEditorWindow) {
				pos = _componentEditorWindow.position;
			
				screenPoint += new Vector2(CalculateSpecificComponentPointX(_selectedIndex), position.height);
				
				diff = Vector3.Distance(pos.position, screenPoint);
				if (diff >= 3) {
					pos.position = screenPoint;

					_componentEditorWindow.position = pos;
					sceneView.Repaint();
				}
			}

			if (_gameObjectWindow) {
				pos = _gameObjectWindow.position;
				screenPoint = CalculateScreenPoint() + new Vector2(0, position.height);
				
				diff = Vector3.Distance(pos.position, screenPoint);
				if (diff >= 3) {
					pos.position = screenPoint;
					// pos.size = new Vector2(300, 46);

					_gameObjectWindow.position = pos;
					sceneView.Repaint();
				}
			}
		}
	}
}