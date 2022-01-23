using UnityEngine;

namespace Nomnom.QuickScene.Editor.Tree.Nodes {
	public class SpacerNode: SimpleNode {
		public override bool CanSelect => false;
		public override bool CanBeSearched => false;

		public override GUIStyle Style => new GUIStyle {
			fixedHeight = _height
		};

		private float _height;

		public SpacerNode(float height) : base(GUIContent.none) {
			_height = height;
		}
	}
}