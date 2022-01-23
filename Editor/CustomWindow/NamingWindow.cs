using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public class NamingWindow: EditorWindow {
		// public static bool InUse;
		//
		// private static NamingWindow _namingWindow;
		// private string _text;
		// private bool _isFocused;
		// private bool _destroyOnClose;
		// private bool _renameOnClose;
		// private bool _constantRename;
		// private GameObject _workingObject;
		//
		// [InitializeOnLoadMethod]
		// private static void OnLoad() {
		// 	if (HasOpenInstances<NamingWindow>()) {
		// 		GetWindow<NamingWindow>().Close();
		// 	}
		// }
		//
		// public static void Init(bool destroyOnCancel, bool constantRename, bool renameOnClose) {
		// 	if (_namingWindow) {
		// 		return;
		// 	}
		// 	
		// 	Vector2 screenPoint = SceneView.lastActiveSceneView.position.position +
		// 	                      HandleUtility.WorldToGUIPoint(Selection.activeGameObject.transform.position);
		// 	NamingWindow window = CreateInstance<NamingWindow>();
		//
		// 	Vector2 size = new Vector2(100 + 40, EditorGUIUtility.singleLineHeight + 4);
		// 	Rect finalRect = new Rect(screenPoint, size);
		// 	window.minSize = size;
		// 	window.maxSize = size;
		// 	window.position = finalRect;
		// 	window._workingObject = Selection.activeGameObject;
		// 	window._destroyOnClose = destroyOnCancel;
		// 	window._renameOnClose = renameOnClose;
		// 	window._constantRename = constantRename;
		// 	window.ShowPopup();
		// 	_namingWindow = window;
		//
		// 	QuickSceneTool.onSceneFrame += window.UpdatePosition;
		// }
		//
		// private void OnEnable() {
		// 	InUse = true;
		// }
		//
		// private void OnDisable() {
		// 	InUse = false;
		// }
		//
		// private void OnLostFocus() {
		// 	if (_destroyOnClose) {
		// 		DestroyImmediate(_workingObject);
		// 	}
		// 	
		// 	CloseWindow();
		// }
		//
		// private void OnProjectChange() {
		// 	Close();
		// }
		//
		// private void UpdatePosition() {
		// 	Vector2 screenPoint = SceneView.lastActiveSceneView.position.position +
		// 	                      HandleUtility.WorldToGUIPoint(_workingObject.transform.position);
		// 	Rect pos = position;
		// 	float diff = Vector3.Distance(pos.position, screenPoint);
		// 	if (diff < 3) {
		// 		return;
		// 	}
		// 	pos.position = screenPoint;
		// 	position = pos;
		// }
		//
		// private void OnGUI() {
		// 	if (!_workingObject ||  Selection.activeGameObject != _workingObject) {
		// 		CloseWindow();
		// 		return;
		// 	}
		//
		// 	GUI.SetNextControlName("input");
		// 	
		// 	Event e = Event.current;
		// 	if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return)) {
		// 		CloseWindow();
		// 		return;
		// 	}
		//
		// 	EditorGUILayout.BeginHorizontal();
		// 	{
		// 		EditorGUI.BeginChangeCheck();
		// 		_text = EditorGUILayout.TextField(_text);
		//
		// 		if (EditorGUI.EndChangeCheck()) {
		// 			if (_constantRename) {
		// 				_workingObject.name = _text;
		// 			}
		// 		}
		//
		// 		if (!_isFocused) {
		// 			_text = Selection.activeGameObject.name;
		// 			_isFocused = true;
		// 			GUI.FocusControl("input");
		// 		}
		//
		// 		GUI.backgroundColor = Color.green;
		// 		if (GUILayout.Button("✔", GUILayout.Width(20))) {
		// 			if (_workingObject) {
		// 				if (_renameOnClose || _constantRename) {
		// 					_workingObject.name = _text;
		// 				}
		// 			}
		// 			
		// 			CloseWindow();
		// 			return;
		// 		}
		// 		
		// 		GUI.backgroundColor = Color.red;
		// 		if (GUILayout.Button("✖", GUILayout.Width(20))) {
		// 			// nuke the object
		// 			if (_destroyOnClose) {
		// 				DestroyImmediate(_workingObject);
		// 			}
		//
		// 			CloseWindow();
		// 		}
		// 	}
		// 	EditorGUILayout.EndHorizontal();
		// }
		//
		// private void CloseWindow() {
		// 	QuickSceneTool.onSceneFrame -= UpdatePosition;
		// 	
		// 	Close();
		// 	QuickSceneTool.Reset();
		// 	
		// 	GetWindow<SceneView>().Focus();
		// }
	}
}