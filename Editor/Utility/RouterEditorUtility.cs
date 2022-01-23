using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Utility {
	internal static class RouterEditorUtility {
		private static MethodInfo _displayObjectContextMenuFunc;
		//private static MethodInfo _displayObjectContextPopupMenuFunc;
		private static MethodInfo _displayObjectContextMenuMultiFunc;
		private static MethodInfo _displayObjectContextPopupMenuMultiFunc;
		
		[InitializeOnLoadMethod]
		private static void OnLoad() {
			_displayObjectContextMenuFunc =
				typeof(EditorUtility).GetMethod(
					"DisplayObjectContextMenu", 
					BindingFlags.NonPublic | BindingFlags.Static, 
					null,
					new [] { typeof(Rect), typeof(Object), typeof(int) }, 
					null
				);
			
			_displayObjectContextMenuMultiFunc =
				typeof(EditorUtility).GetMethod(
					"DisplayObjectContextMenu", 
					BindingFlags.NonPublic | BindingFlags.Static, 
					null,
					new [] { typeof(Rect), typeof(Object[]), typeof(int) }, 
					null
				);
			
			// _displayObjectContextPopupMenuFunc =
			// 	typeof(EditorUtility).GetMethod(
			// 		"DisplayObjectContextPopupMenu", 
			// 		BindingFlags.NonPublic | BindingFlags.Static, 
			// 		null,
			// 		new [] { typeof(Rect), typeof(Object), typeof(int) }, 
			// 		null
			// 	);
			
			_displayObjectContextPopupMenuMultiFunc =
				typeof(EditorUtility).GetMethod(
					"DisplayObjectContextPopupMenu", 
					BindingFlags.NonPublic | BindingFlags.Static, 
					null,
					new [] { typeof(Rect), typeof(Object[]), typeof(int) }, 
					null
				);
		}

		public static void DisplayObjectContextMenu(Rect rect, Object[] targets, int contextUserData) {
			if (targets.Length == 1) {
				_displayObjectContextMenuFunc.Invoke(null, new object[] {rect, targets[0], contextUserData});
				return;
			}
			
			_displayObjectContextMenuMultiFunc.Invoke(null, new object[] {rect, targets, contextUserData});
		}
		
		public static void DisplayObjectContextPopupMenu(Rect rect, Object[] targets, int contextUserData) {
			// if (targets.Length == 1) {
			// 	_displayObjectContextPopupMenuFunc.Invoke(null, new object[] {rect, targets[0], contextUserData});
			// 	return;
			// }
			
			_displayObjectContextPopupMenuMultiFunc.Invoke(null, new object[] {rect, targets, contextUserData});
		}
	}
}