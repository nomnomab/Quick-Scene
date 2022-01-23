using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public class GameObjectWindow: QuickWindow<GameObjectWindow>, IDisposable {
		private static GUIStyle _dropdownIconStyle;
		private static Texture _dropdownIcon;
		private static string[] _tags;
		private static string[] _layers;
		private static Dictionary<int, int> _remapper;

		private GameObject _sceneObject;
		
		public static GameObjectWindow Open(GameObject obj, Vector2 screenCoords, Vector2 size) {
			Open(screenCoords, size);

			Instance.Init(obj);

			return Instance;
		}

		public override void Dispose() {
			_sceneObject = null;
		}

		public override void Init() {
			_dropdownIconStyle ??= new GUIStyle {
				fixedWidth = 18,
				padding = {
					top = 5,
					left = 2
				}
			};
			
			if (!_dropdownIcon) {
				_dropdownIcon = DataCache.GetIcon("icon dropdown");
			}
		}

		public void Init(GameObject obj) {
			_sceneObject = obj;
			_tags = UnityEditorInternal.InternalEditorUtility.tags;
			List<string> layerList = new List<string>();
			_remapper = new Dictionary<int, int>();

			for (int i = 0; i < 32; i++) {
				string name = LayerMask.LayerToName(i);
				
				if (string.IsNullOrEmpty(name)) {
					continue;
				}
				
				layerList.Add($"{i}: {name}");
				_remapper[i] = layerList.Count - 1;
			}

			_layers = layerList.ToArray();
		}

		public override void ShowAs(Rect rect, Vector2 size) {
			position = new Rect(rect.position, size);
			minSize = maxSize = size;
			
			ShowPopup();
		}
		
		public override void OnObjectChanged(GameObject oldObj, GameObject newObj) {
			// when an object is changed, nuke this window
			OnClose();
		}
		
		public override bool OnPreGUI(Event e) {
			// validate items
			return _sceneObject;
		}

		public override void OnDrawGUI(Event e) {
			// first row
			bool enabled = _sceneObject.activeSelf;
			string name = _sceneObject.name;
			bool isStatic = _sceneObject.isStatic;
			// StaticEditorFlags staticFlags = GameObjectUtility.GetStaticEditorFlags(_sceneObject);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			bool newEnabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(14));
			string newName = EditorGUILayout.TextField(name, GUILayout.ExpandWidth(true));
			bool newIsStatic = EditorGUILayout.ToggleLeft("Static", isStatic, GUILayout.Width(50));
			
			if (GUILayout.Button(_dropdownIcon, _dropdownIconStyle, GUILayout.Width(14))) {
				// show static flags
			}
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(6);
			
			// second row
			EditorGUILayout.BeginHorizontal();
			string tag = _sceneObject.tag;
			int layer = _sceneObject.layer;

			int tagIndex = 0;

			for (int i = 0; i < _tags.Length; i++) {
				if (_tags[i] == tag) {
					tagIndex = i;
					break;
				}
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Tag", GUILayout.Width(24));
			string newTag = _tags[EditorGUILayout.Popup(tagIndex, _tags)];
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Layer", GUILayout.Width(34));
			string layerStr = _layers[EditorGUILayout.Popup(_remapper[layer], _layers)];
			int newLayer = int.Parse(layerStr.Substring(0, layerStr.IndexOf(':')));
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.EndHorizontal();
			
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(_sceneObject, "Modified GameObject");
				
				if (enabled != newEnabled) {
					_sceneObject.SetActive(newEnabled);
				}

				if (name != newName) {
					_sceneObject.name = newName;
				}

				if (isStatic != newIsStatic) {
					_sceneObject.isStatic = newIsStatic;
				}

				if (!_sceneObject.CompareTag(newTag)) {
					_sceneObject.tag = newTag;
				}

				if (_sceneObject.layer != newLayer) {
					_sceneObject.layer = newLayer;
				}
				
				EditorUtility.SetDirty(_sceneObject);
			}
		}
	}
}