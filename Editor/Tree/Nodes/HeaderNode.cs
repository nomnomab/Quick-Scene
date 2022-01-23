using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Tree.Nodes {
	public class HeaderNode: SimpleNode {
		public override bool CanSelect => false;
		public override bool CanBeSearched => false;
		public override GUIStyle Style => new GUIStyle("DD ItemStyle") {
			padding = {
				left = 8,
				right = 2,
				top = 2,
				bottom = 2
			},
			fixedHeight = EditorGUIUtility.singleLineHeight + 9,
			fontStyle = FontStyle.Bold,
			fontSize = 14
		};

		public HeaderNode(GUIContent label) : base(label) { }
		public HeaderNode(string label) : base(label) { }
	}
}