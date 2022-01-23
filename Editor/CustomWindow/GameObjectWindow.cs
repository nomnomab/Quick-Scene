using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public class GameObjectWindow: QuickWindow<GameObjectWindow>, IDisposable {
		private static GUIStyle _dropdownIconStyle;
		private static Texture _dropdownIcon;
		private static string[] _tags;
		private static string[] _layers;
		private static Dictionary<int, int> _remapper;

		private GameObject[] _sceneObjects;
		
		public static GameObjectWindow Open(GameObject[] obj, Vector2 screenCoords, Vector2 size) {
			Open(screenCoords, size);

			Instance.Init(obj);

			return Instance;
		}

		public override void Dispose() {
			_sceneObjects = null;
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

		public void Init(GameObject[] obj) {
			_sceneObjects = obj;
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
			return _sceneObjects != null && _sceneObjects.Length > 0 && _sceneObjects.Any(s => s);
		}

		public override void OnDrawGUI(Event e) {
			// if we have a multi-selection we need to basically show a completely different value ui until something changes
			DrawNormal();
		}

		private void DrawNormal() {
			State state = default;
			EditorGUI.BeginChangeCheck();
			
			// first row
			EditorGUILayout.BeginHorizontal();
			bool enabled = DrawStateToggle(ref state);
			string name = DrawName(ref state);
			bool isStatic = DrawStatic(ref state);
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(6);
			
			// second row
			EditorGUILayout.BeginHorizontal();
			string tag = DrawTag(ref state);
			int layer = DrawLayer(ref state);
			EditorGUILayout.EndHorizontal();
			
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObjects(_sceneObjects.Select(s => (UnityEngine.Object)s).ToArray(), "Modified GameObject(s)");

				foreach (GameObject obj in _sceneObjects) {
					if (state.hasNewState && enabled != obj.activeSelf) {
						obj.SetActive(enabled);
					}
			
					if (state.hasNewName && name != obj.name) {
						obj.name = name;
					}
			
					if (state.hasNewStatic && isStatic != obj.isStatic) {
						obj.isStatic = isStatic;
					}
			
					if (state.hasNewTag && !obj.CompareTag(tag)) {
						obj.tag = tag;
					}
			
					if (state.hasNewLayer && obj.layer != layer) {
						obj.layer = layer;
					}
				
					EditorUtility.SetDirty(obj);
				}
			}
		}

		private bool DrawStateToggle(ref State state) {
			bool enabled = _sceneObjects[0].activeSelf;
			bool allSame = true;
			foreach (GameObject sceneObject in _sceneObjects) {
				if (sceneObject.activeSelf == enabled) {
					continue;
				}

				allSame = false;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = !allSame;
			bool newEnabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(14));
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck()) {
				state.hasNewState = true;
			}

			return newEnabled;
		}

		private string DrawName(ref State state) {
			string name = _sceneObjects[0].name;
			bool allSame = true;
			foreach (GameObject sceneObject in _sceneObjects) {
				if (sceneObject.name == name) {
					continue;
				}

				allSame = false;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = !allSame;
			string newName = EditorGUILayout.TextField(name, GUILayout.ExpandWidth(true));
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck()) {
				state.hasNewName = true;
			}

			return newName;
		}

		private bool DrawStatic(ref State state) {
			bool isStatic = _sceneObjects[0].isStatic;
			bool allSame = true;
			foreach (GameObject sceneObject in _sceneObjects) {
				if (sceneObject.isStatic == isStatic) {
					continue;
				}

				allSame = false;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = !allSame;
			bool newStatic = EditorGUILayout.ToggleLeft("Static", isStatic, GUILayout.Width(50));
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck()) {
				state.hasNewStatic = true;
			}
			
			if (GUILayout.Button(_dropdownIcon, _dropdownIconStyle, GUILayout.Width(14))) {
				
			}

			return newStatic;
		}

		private string DrawTag(ref State state) {
			string tag = _sceneObjects[0].tag;
			bool allSame = true;
			foreach (GameObject sceneObject in _sceneObjects) {
				if (sceneObject.CompareTag(tag)) {
					continue;
				}

				allSame = false;
			}
			
			int tagIndex = 0;
			
			for (int i = 0; i < _tags.Length; i++) {
				if (_tags[i] != tag) {
					continue;
				}
				
				tagIndex = i;
				break;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = !allSame;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Tag", GUILayout.Width(24));
			string newTag = _tags[EditorGUILayout.Popup(tagIndex, _tags)];
			EditorGUILayout.EndHorizontal();
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck()) {
				state.hasNewTag = true;
			}
			
			return newTag;
		}

		private int DrawLayer(ref State state) {
			int layer = _sceneObjects[0].layer;
			
			bool allSame = true;
			foreach (GameObject sceneObject in _sceneObjects) {
				if (sceneObject.layer == layer) {
					continue;
				}

				allSame = false;
			}
			
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = !allSame;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Layer", GUILayout.Width(34));
			string layerStr = _layers[EditorGUILayout.Popup(_remapper[layer], _layers)];
			int newLayer = int.Parse(layerStr.Substring(0, layerStr.IndexOf(':')));
			EditorGUILayout.EndHorizontal();
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck()) {
				state.hasNewLayer = true;
			}

			return newLayer;
		}

		private struct State {
			public bool hasNewState;
			public bool hasNewName;
			public bool hasNewStatic;
			public bool hasNewTag;
			public bool hasNewLayer;
		}
	}
}