using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nomnom.QuickScene.Editor.Utility {
	internal static class RouterEditor {
		private static MethodInfo _drawHeaderFromInsideHierarchyFunc;
		private static MethodInfo _onHeaderGUIFunc;
		private static MethodInfo _onHeaderControlsGUIFunc;
		private static MethodInfo _onHeaderIconGUIFunc;
		private static MethodInfo _drawPostIconContentFunc;
		private static MethodInfo _drawHeaderHelpAndSettingsGUIFunc;
		private static MethodInfo _onHeaderTitleGUIFunc;
		private static PropertyInfo _targetTitleProperty;
		private static Object _mainWindow;
		private static PropertyInfo _mainWindowPositionProperty;

		public static class EditorStyles {
			public static GUIStyle inspectorBig =
				(GUIStyle) typeof(UnityEditor.EditorStyles)
					.GetProperty("inspectorBig", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

			public static GUIStyle inspectorTitlebar =
				(GUIStyle) typeof(UnityEditor.EditorStyles)
					.GetProperty("inspectorTitlebar", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

			public static GUIStyle inspectorTitlebarFlat =
				(GUIStyle) typeof(UnityEditor.EditorStyles)
					.GetProperty("inspectorTitlebarFlat", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

			public static GUIStyle inspectorTitlebarText =
				(GUIStyle) typeof(UnityEditor.EditorStyles)
					.GetProperty("inspectorTitlebarText", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		}

		public static class BaseStyles {
			public static readonly GUIContent open = EditorGUIUtility.TrTextContent("Open");
			public static readonly GUIStyle inspectorBig = new GUIStyle(EditorStyles.inspectorBig);
			public static readonly GUIStyle centerStyle = new GUIStyle();
			public static readonly GUIStyle postLargeHeaderBackground = (GUIStyle) "IN BigTitle Post";

			static BaseStyles() => centerStyle.alignment = TextAnchor.MiddleCenter;
		}

		[InitializeOnLoadMethod]
		private static void OnLoad() {
			Type ty = typeof(UnityEditor.Editor);

			_drawHeaderFromInsideHierarchyFunc = ty.GetMethod("DrawHeaderFromInsideHierarchy",
				BindingFlags.NonPublic | BindingFlags.Instance);

			_onHeaderGUIFunc =
				ty.GetMethod("OnHeaderGUI", BindingFlags.NonPublic | BindingFlags.Instance);

			_onHeaderControlsGUIFunc =
				ty.GetMethod("OnHeaderControlsGUI", BindingFlags.NonPublic | BindingFlags.Instance);

			_onHeaderIconGUIFunc =
				ty.GetMethod("OnHeaderIconGUI", BindingFlags.NonPublic | BindingFlags.Instance);

			_drawPostIconContentFunc =
				ty.GetMethod(
					"DrawPostIconContent",
					BindingFlags.NonPublic | BindingFlags.Instance, null,
					new[] {typeof(Rect)},
					null
				);

			_drawHeaderHelpAndSettingsGUIFunc =
				ty.GetMethod("DrawHeaderHelpAndSettingsGUI", BindingFlags.NonPublic | BindingFlags.Instance);

			_onHeaderTitleGUIFunc = ty.GetMethod("OnHeaderTitleGUI", BindingFlags.NonPublic | BindingFlags.Instance);

			_targetTitleProperty = ty.GetProperty("targetTitle", BindingFlags.NonPublic | BindingFlags.Instance);

			var containerType = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject))
				.Where(t => t.Name == "ContainerWindow").FirstOrDefault();
			if (containerType == null)
				throw new System.MissingMemberException(
					"Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
			var showModeField = containerType.GetField("m_ShowMode",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			_mainWindowPositionProperty = containerType.GetProperty("position",
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (showModeField == null || _mainWindowPositionProperty == null)
				throw new System.MissingFieldException(
					"Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
			var windows = Resources.FindObjectsOfTypeAll(containerType);

			foreach (var win in windows) {
				var showmode = (int) showModeField.GetValue(win);
				if (showmode == 4) // main window
				{
					_mainWindow = win;
				}
			}
		}

		public static void DrawHeaderFromInsideHierarchy(UnityEditor.Editor editor) {
			_drawHeaderFromInsideHierarchyFunc.Invoke(editor, null);
		}

		// public static void OnHeaderGUI(UnityEditor.Editor editor, GUIStyle style) {
		// 	DrawHeaderGUI(editor, (string) _targetTitleProperty.GetValue(editor), 0, style);
		// }

		public static string GetTitleProperty(UnityEditor.Editor editor) {
			return (string) _targetTitleProperty.GetValue(editor);
		}

		public static void OnHeaderControlsGUI(UnityEditor.Editor editor) {
			_onHeaderControlsGUIFunc.Invoke(editor, null);
		}

		public static void OnHeaderIconGUI(UnityEditor.Editor editor, Rect rect) {
			_onHeaderIconGUIFunc.Invoke(editor, new object[] {rect});
		}

		public static void DrawPostIconContent(UnityEditor.Editor editor, Rect rect) {
			_drawPostIconContentFunc.Invoke(editor, new object[] {rect});
		}

		public static Rect DrawHeaderHelpAndSettingsGUI(UnityEditor.Editor editor, Rect rect) {
			return (Rect) _drawHeaderHelpAndSettingsGUIFunc.Invoke(editor, new object[] {rect});
		}

		public static void OnHeaderTitleGUI(UnityEditor.Editor editor, Rect rect, string header) {
			_onHeaderTitleGUIFunc.Invoke(editor, new object[] {rect, header});
		}

		public static Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType) {
			var result = new List<System.Type>();
			var assemblies = aAppDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				var types = assembly.GetTypes();
				foreach (var type in types) {
					if (type.IsSubclassOf(aType))
						result.Add(type);
				}
			}

			return result.ToArray();
		}

		public static Rect GetEditorMainWindowPos() {
			Rect pos = (Rect) _mainWindowPositionProperty.GetValue(_mainWindow, null);
			return pos;
		}

		public static bool ShowAddComponentWindow(Rect rect, params GameObject[] gos) {
			Type ty = Type.GetType(
				"UnityEditor.AddComponent.AddComponentWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			return (bool)ty.GetMethod("Show", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { rect, gos });
		}
	}
}