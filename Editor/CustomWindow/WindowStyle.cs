using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public class WindowStyle {
		// styles
		public GUIStyle HeaderStyle = new GUIStyle("DD HeaderStyle");
		// public GUIStyle CenteredHeaderTextStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
		// 	normal = {
		// 		textColor = Color.white * 0.8f
		// 	}
		// };
		public GUIStyle ItemStyle = new GUIStyle("DD ItemStyle") {
			padding = {
				left = 30,
				right = 2,
				top = 2,
				bottom = 2
			},
			fixedHeight = EditorGUIUtility.singleLineHeight + 9,
			richText = true
		};
		public GUIStyle LineSeparatorStyle = new GUIStyle("DefaultLineSeparator");
		public GUIStyle LeftArrowStyle = new GUIStyle("ArrowNavigationLeft");
		public GUIStyle RightArrowStyle = new GUIStyle("ArrowNavigationRight");
		
		// content
		public GUIContent CheckMarkContent = new GUIContent("✔");
		
		// colors
		public Color BackgroundColorLight = Color.white;
		public Color BackgroundColorDark = Color.black * 0.7f;

		public WindowStyle() {}
		
		public WindowStyle(Color backgroundColorLight, Color backgroundColorDark) {
			BackgroundColorLight = backgroundColorLight;
			BackgroundColorDark = backgroundColorDark;
		}

		public Color GetBackgroundColor() {
			return EditorGUIUtility.isProSkin ? BackgroundColorDark : BackgroundColorLight;
		}
	}
}