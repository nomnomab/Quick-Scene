using System.Collections.Generic;
using Nomnom.QuickScene.Editor.CustomWindow;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Tree.Nodes {
	public interface INode {
		uint Owner { get; set; }
		uint Id { get; set; }
		bool CanSelect { get; }
		bool CanBeSearched { get; }
		GUIContent Label { get; set; }
		GUIStyle Style { get; }
		List<INode> Children { get; set; }

		void Draw(Rect rect, bool isHovering, WindowStyle styles);
		void OnClicked();
	}
}