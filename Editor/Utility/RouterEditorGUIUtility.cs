using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Utility {
	internal static class RouterEditorGUIUtility {
		public static float contextWidth => (float)_contextWidthProperty.GetValue(null);
		
		public static GUIStyle topLevelStyle 
			=> (GUIStyle)_topLevelStyleProperty.GetValue(_topLevelObjectProperty.GetValue(null));

		private static PropertyInfo _topLevelObjectProperty;
		private static PropertyInfo _topLevelStyleProperty;
		private static PropertyInfo _contextWidthProperty;
		
		[InitializeOnLoadMethod]
		private static void OnLoad() {
			Type guiLayoutEntryType =
				Type.GetType(
					"UnityEngine.GUILayoutEntry, UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			
			_topLevelObjectProperty =
				typeof(GUILayoutUtility).GetProperty("topLevel", BindingFlags.Static | BindingFlags.NonPublic);
			
			_topLevelStyleProperty = 
				guiLayoutEntryType.GetProperty("style", BindingFlags.Public | BindingFlags.Instance);
			
			_contextWidthProperty =
				typeof(EditorGUIUtility).GetProperty("contextWidth", BindingFlags.NonPublic | BindingFlags.Static);
		}
	}
}