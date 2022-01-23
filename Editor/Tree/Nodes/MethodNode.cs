using System;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Tree.Nodes {
	public class MethodNode: SimpleNode {
		private Action _onClicked;

		public MethodNode(Action onClicked, GUIContent label) : base(label) {
			_onClicked = onClicked;
		}

		public MethodNode(Action onClicked, string label) : this(onClicked, new GUIContent(label)) { }

		public MethodNode(Action onClicked, string label, Texture texture) : this(onClicked, new GUIContent(label, texture)) { }

		public override void OnClicked() {
			base.OnClicked();
			
			_onClicked?.Invoke();
		}
	}
}