using System;
using Nomnom.QuickScene.Editor.CustomWindow;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Nomnom.QuickScene.Editor {
	internal class QuickSceneTool: EditorWindow {
		public static event Action onSceneFrameDelay;

		private static QuickSceneInput _input;
		private static ScenePlane _plane;
		
		private static GameObject _selectedObject;
		private static bool _selectedObjectLastLifetime;
		
		// windows
		private static AddWindow _addWindow;
		private static FloatingHeaderWindow _floatingHeaderWindow;
		
		[InitializeOnLoadMethod]
		private static void OnLoad() {
			_input = new QuickSceneInput();
			_plane = new ScenePlane();
			
			SceneView.duringSceneGui -= OnScene;
			SceneView.duringSceneGui += OnScene;

			AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
			
			ObjectChangeEvents.changesPublished -= OnObjectChanged;
			ObjectChangeEvents.changesPublished += OnObjectChanged;

			EditorApplication.wantsToQuit -= OnEditorQuit;
			EditorApplication.wantsToQuit += OnEditorQuit;

			onSceneFrameDelay = null;
		}

		private static bool OnEditorQuit() {
			if (_addWindow) {
				_addWindow.OnClose();
				_addWindow = null;
			}
			
			if (_floatingHeaderWindow) {
				_floatingHeaderWindow.OnClose();
				_floatingHeaderWindow = null;
			}

			return true;
		}

		private static void OnObjectChanged(ref ObjectChangeEventStream stream) {
			_addWindow?.OnHierarchyChanged(ref stream);
			_floatingHeaderWindow?.OnHierarchyChanged(ref stream);
		}

		private static void OnBeforeAssemblyReload() {
			_addWindow?.OnPreRecompile();
			_floatingHeaderWindow?.OnPreRecompile();
		}

		private void OnDestroy() {
			SceneView.duringSceneGui += OnScene;
		}

		private static void OnScene(SceneView sceneView) {
			Event e = Event.current;

			// handle input
			_input.Update(e);

			// handle events
			
			// fires once then nukes all attachments
			onSceneFrameDelay?.Invoke();
			onSceneFrameDelay = null;
			
			// used to run shit on the scene taskbar
			_addWindow?.OnSceneFrame(sceneView);
			_floatingHeaderWindow?.OnSceneFrame(sceneView);
			
			HandleSelectionChanged(e);
			HandleGizmo(e, sceneView);

			_selectedObjectLastLifetime = _selectedObject;
		}

		private static void HandleSelectionChanged(Event e) {
			GameObject newObject = Selection.activeGameObject;

			if (_selectedObject == newObject) {
				// was not null last frame
				if (!_selectedObject && !_selectedObjectLastLifetime || _selectedObject && newObject) {
					return;
				}
			}

			_addWindow?.OnObjectChanged(_selectedObject, newObject);
			_floatingHeaderWindow?.OnObjectChanged(_selectedObject, newObject);

			_selectedObject = newObject;

			if (!_selectedObject) {
				return;
			}
			
			Vector2 screenCoords = GUIUtility.GUIToScreenPoint(e.mousePosition);
			_floatingHeaderWindow = FloatingHeaderWindow.Open(Selection.gameObjects, screenCoords, Vector2.zero);
			_floatingHeaderWindow.onClosed += OnFloatingWindowClosed;
		}

		private static void HandleGizmo(Event e, SceneView sceneView) {
			if (!_input.ShowOrigin && !_input.ShowPointer || _addWindow) {
				return;
			}
			
			// handle plane
			_plane.Update(e, sceneView);
			
			const float WANTED_SIZE = 0.02f;
			Vector3 point = _plane.Point;
			float distance = Vector3.Distance(point, _plane.Origin);
			float size = WANTED_SIZE * distance;
				
			if (_input.M0) {
				// open window
				Vector2 screenCoords = GUIUtility.GUIToScreenPoint(e.mousePosition);

				if (_input.ShowOrigin) {
					point = Vector3.zero;
				}
				
				_addWindow = AddWindow.Open(new WindowState(point, _plane.Normal, _plane.Surface), screenCoords, new Vector2(250, 325));
				_addWindow.onClosed += OnAddWindowClosed;
				
				_input.Reset();
				
				e.Use();
			}

			Handles.ScaleHandle(Vector3.zero, point, Quaternion.identity, size);

			if (_input.ShowOrigin) {
				Handles.DrawDottedLine(point, Vector3.zero, 4);
			}

			sceneView.Repaint();
		}

		// events
		private static void OnAddWindowClosed() {
			_addWindow.onClosed -= OnAddWindowClosed;
			_addWindow = null;
		}

		private static void OnFloatingWindowClosed() {
			_floatingHeaderWindow.onClosed -= OnFloatingWindowClosed;
			_floatingHeaderWindow = null;
		}
	}
}