using System;
using Nomnom.QuickScene.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public class ComponentEditorWindow : QuickWindow<ComponentEditorWindow>, IDisposable {
		private Component _component;
		private UnityEditor.Editor _editor;
		private Vector2 _scrollbar;

		public static ComponentEditorWindow Open(Component component, Vector2 screenCoords, Vector2 size) {
			Open(screenCoords, size);

			Instance.Init(component);
			
			return Instance;
		}

		public override void Dispose() {
			if (_editor) {
				_editor.ResetTarget();
			}

			DestroyImmediate(_editor);
			
			_component = null;
			_editor = null;
		}

		public override void Init() { }

		public void Init(Component component) {
			_component = component;
			_editor = UnityEditor.Editor.CreateEditor(component);
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
			return _component;
		}

		public override void OnDrawGUI(Event e) {
			// draw header
			Rect headerRect = DrawHeaderGUI(_editor);
			float height = headerRect.height;

			_scrollbar = EditorGUILayout.BeginScrollView(_scrollbar);
			{
				_editor.OnInspectorGUI();

				if (e.type == EventType.Repaint) {
					height += GUILayoutUtility.GetLastRect().height;
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