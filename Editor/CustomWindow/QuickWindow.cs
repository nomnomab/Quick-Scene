using System;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public abstract class QuickWindow<T>: EditorWindow where T: QuickWindow<T>, IDisposable {
		public event Action onClosed; 

		public static T Instance { get; private set; }

		public static T Open(Vector2 screenCoords, Vector2 size) {
			if (Instance) {
				Instance.OnClose();
			}
			
			T window = CreateInstance<T>();
			Instance = window;
			
			Rect finalRect = new Rect(screenCoords, Vector2.zero);
			
			window.position = finalRect;
			window.ShowAs(finalRect, size);
			window.Init();

			return window;
		}

		private void OnDestroy() {
			if (Instance) {
				Dispose();
				Instance = null;
				onClosed?.Invoke();
			}
		}

		/// <summary>
		/// Disposes of any data left over.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// Called when closing the window. Override and call base.OnClose() to finalize the closure.
		/// </summary>
		public virtual void OnClose() {
			Dispose();
			Instance.Close();
			Instance = null;
			
			onClosed?.Invoke();
		}

		public abstract void Init();
		
		public abstract void ShowAs(Rect rect, Vector2 size);

		private void OnGUI() {
			Event e = Event.current;
			
			if (!OnPreGUI(e)) {
				return;
			}
			
			OnDrawGUI(e);
			OnAfterGUI(e);
		}

		public abstract void OnDrawGUI(Event e);

		/// <summary>
		/// Runs before OnGUI. If this returns false, it will quit early.
		/// </summary>
		public virtual bool OnPreGUI(Event e) {
			return Instance;
		}

		public virtual void OnAfterGUI(Event e) { }

		public virtual void Update() { }

		public virtual void OnPreRecompile() {
			OnClose();
		}
		
		public virtual void OnHierarchyChanged(ref ObjectChangeEventStream stream) {}
		
		public virtual void OnObjectChanged(GameObject oldObj, GameObject newObj){}
		
		public virtual void OnSceneFrame(SceneView sceneView) {}
	}
}