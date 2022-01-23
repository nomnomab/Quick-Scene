using Nomnom.QuickScene.Editor.CustomWindow;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Tree.Nodes {
	public class AssetNode: SpawnNode {
		public AssetNode(string assetPath, GUIContent label) : base(assetPath, label) { }
		public AssetNode(string assetPath, string label) : base(assetPath, label) { }
		public AssetNode(string assetPath, string label, Texture texture) : base(assetPath, label, texture) { }
		
		private GUIStyle _pathStyle => new GUIStyle("DD ItemStyle") {
			padding = {
				left = 30,
				right = 2,
				top = 2,
				bottom = 2
			},
			fixedHeight = EditorGUIUtility.singleLineHeight + 9,
			fontSize = 9,
			normal = {
				textColor = Color.gray
			}
		};

		public override void Draw(Rect rect, bool isHovering, WindowStyle styles) {
			base.Draw(rect, isHovering, styles);
			
			// draw asset path to the right
			Rect pathRect = new Rect(Style.CalcSize(new GUIContent(Label.text)).x - 24, rect.y, Screen.width, rect.height);
			_pathStyle.Draw(pathRect, new GUIContent(AssetPath), false, false, false, false);
		}
	}
}