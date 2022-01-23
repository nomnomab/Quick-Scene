using System.Collections.Generic;
using Nomnom.QuickScene.Editor.Tree;
using Nomnom.QuickScene.Editor.Tree.Nodes;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	public class WindowState {
		public readonly Vector3 HitPoint;
		public readonly Vector3 HitNormal;
		public readonly Transform Surface;
		
		public WindowStyle Style;
		public NodeTree Tree;
		public INode CurrentNode;
		public Stack<INode> History;

		public WindowState(Vector3 hitPoint, Vector3 hitNormal, Transform surface) {
			HitPoint = hitPoint;
			HitNormal = hitNormal;
			Surface = surface;
			
			Style = new WindowStyle();
			Tree = DataCache.DefaultTree;
			CurrentNode = Tree.Find(0);
			History = new Stack<INode>();
		}

		public WindowState SetStyle(WindowStyle style) {
			Style = style;
			return this;
		}

		public WindowState SetTree(NodeTree tree) {
			Tree = tree;
			return this;
		}
	}
}