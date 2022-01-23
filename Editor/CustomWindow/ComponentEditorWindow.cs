using System;
using System.Linq;
using Nomnom.QuickScene.Editor.Utility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public class ComponentEditorWindow : QuickWindow<ComponentEditorWindow>, IDisposable {
		private Object[] _components;
		private UnityEditor.Editor _editor;
		private MaterialEditor _materialEditor;
		private Vector2 _scrollbar;

		public static ComponentEditorWindow Open(Component[] components, Vector2 screenCoords, Vector2 size) {
			Open(screenCoords, size);

			Instance.Init(components);

			return Instance;
		}

		public override void Dispose() {
			if (_editor) {
				_editor.ResetTarget();
			}

			DestroyImmediate(_editor);

			if (_materialEditor) {
				_materialEditor.ResetTarget();
			}

			DestroyImmediate(_materialEditor);

			_components = null;
			_editor = null;
			_materialEditor = null;
		}

		public override void Init() { }

		public void Init(params Object[] components) {
			_components = components;

			_editor = UnityEditor.Editor.CreateEditor(components);

			// check if all materials are the same
			if (components[0] is Renderer renderer) {
				if (components.All(c => ((Renderer) c).sharedMaterial == renderer.sharedMaterial)) {
					_materialEditor = (MaterialEditor) UnityEditor.Editor.CreateEditor(renderer.sharedMaterial);
				}
			}
		}

		public override void ShowAs(Rect rect, Vector2 size) {
			position = new Rect(rect.position, size);

			ShowPopup();
		}

		public override void OnObjectChanged(GameObject oldObj, GameObject newObj) {
			// when an object is changed, nuke this window
			OnClose();
		}

		public override bool OnPreGUI(Event e) {
			// validate items
			return _components != null && _components.Any(c => c);
		}

		public override void OnDrawGUI(Event e) {
			// draw header
			Rect headerRect = DrawHeaderGUI(_editor);
			float height = headerRect.height;

			_scrollbar = EditorGUILayout.BeginScrollView(_scrollbar);
			{
				_editor.OnInspectorGUI();
				_editor.Repaint();

				if (e.type == EventType.Repaint) {
					Rect mainRect = GUILayoutUtility.GetLastRect();
					height += mainRect.height;
				}

				if (_materialEditor && _editor.target is Renderer renderer) {
					GUILayout.BeginVertical();
					_materialEditor.DrawHeader();
				
					if (_materialEditor.target != renderer.sharedMaterial) {
						_materialEditor.ResetTarget();
						DestroyImmediate(_materialEditor);
				
						_materialEditor = (MaterialEditor) UnityEditor.Editor.CreateEditor(renderer.sharedMaterial);
					}
				
					bool isDefaultMaterial = !AssetDatabase.GetAssetPath(_materialEditor.target).StartsWith("Assets");
					using (new EditorGUI.DisabledGroupScope(isDefaultMaterial)) {
						_materialEditor.OnInspectorGUI();
					}
				
					GUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndScrollView();

			SerializedObject obj = _editor.serializedObject;
			SerializedProperty iter = obj.GetIterator();

			// get the height per property and estimate an overall height
			const string HEADER = "m_Script";
			for (bool enterChildren = true; iter.NextVisible(enterChildren); enterChildren = false) {
				using (new EditorGUI.DisabledScope(HEADER == iter.propertyPath)) {
					height += EditorGUIUtility.singleLineHeight;
				}
			}

			if (e.type == EventType.Repaint && Mathf.Abs(position.height - height) > 3) {
				// if the height has peaked a threshold, update
				// update both min and max since Unity's position system sucks ass
				minSize = maxSize = new Vector2(position.width, height);
			}
		}

		public override void OnAfterGUI(Event e) {
			if (_components[0] is Renderer oneRenderer) {
				if (_components.All(c => ((Renderer) c).sharedMaterial.GetInstanceID() == oneRenderer.sharedMaterial.GetInstanceID())) {
					if (!_materialEditor) {
						_materialEditor = (MaterialEditor) UnityEditor.Editor.CreateEditor(oneRenderer.sharedMaterial);
						
						_editor.ResetTarget();
						DestroyImmediate(_editor);
						_editor = UnityEditor.Editor.CreateEditor(_components);
						_editor.Repaint();
					}
				} else {
					if (_materialEditor) {
						_materialEditor.ResetTarget();
						DestroyImmediate(_materialEditor);
						_materialEditor = null;
						
						_editor.ResetTarget();
						DestroyImmediate(_editor);
						_editor = UnityEditor.Editor.CreateEditor(_components);
						_editor.Repaint();
					}
				}
			}
		}

		/// <summary>
		/// Reimplementation of Unity's DrawHeaderGUI so it could be fucking styled. >:(
		/// </summary>
		private static Rect DrawHeaderGUI(UnityEditor.Editor editor) {
			string header = RouterEditor.GetTitleProperty(editor);

			GUILayout.BeginHorizontal(RouterEditor.EditorStyles.inspectorTitlebar);
			GUILayout.Space(38f);
			GUILayout.BeginVertical();
			GUILayout.Space(21f);
			GUILayout.BeginHorizontal();

			if (editor) {
				RouterEditor.OnHeaderControlsGUI(editor);
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			Rect lastRect = GUILayoutUtility.GetLastRect();
			Rect r = new Rect(lastRect.x, lastRect.y, lastRect.width, lastRect.height);
			Rect rect1 = new Rect(r.x + 6f, r.y + 6f, 32f, 32f);

			GUIStyle tmpStyle = new GUIStyle(RouterEditor.BaseStyles.centerStyle);
			tmpStyle.fixedHeight = 16;
			GUI.Label(rect1, AssetPreview.GetMiniTypeThumbnail(editor.target.GetType()), tmpStyle);

			if (editor) {
				RouterEditor.DrawPostIconContent(editor, rect1);
			}

			float lineHeight = EditorGUIUtility.singleLineHeight;
			Rect rect2;

			if (editor) {
				Rect rect3 = RouterEditor.DrawHeaderHelpAndSettingsGUI(editor, r);
				float x = r.x + 44f;

				rect2 = new Rect(x, r.y + 6f, rect3.x - x - 4, lineHeight);
			} else {
				rect2 = new Rect(r.x + 44f, r.y + 6f, r.width - 44f, lineHeight);
			}

			if (editor) {
				RouterEditor.OnHeaderTitleGUI(editor, rect2, header);
			} else {
				GUI.Label(rect2, header, UnityEditor.EditorStyles.largeLabel);
			}

			bool enabled = GUI.enabled;
			GUI.enabled = true;

			Event e = Event.current;
			bool flag = editor && e.type == EventType.MouseDown && e.button == 1 && r.Contains(e.mousePosition);
			GUI.enabled = enabled;

			if (flag) {
				RouterEditorUtility.DisplayObjectContextMenu(new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0),
					editor.targets, 0);
				e.Use();
			}

			return lastRect;
		}
	}
}