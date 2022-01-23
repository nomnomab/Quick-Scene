using System.Collections.Generic;
using Nomnom.QuickScene.Editor.CustomWindow;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Tree.Nodes {
	/// <summary>
	/// Defines a standard node with only text and an icon.
	/// </summary>
	public class SimpleNode: INode {
		public uint Owner { get; set; }
		public uint Id { get; set; }
		public List<INode> Children { get; set; }
		public GUIContent Label { get; set; }
		public virtual bool CanSelect => true;
		public virtual bool CanBeSearched => true;
		
		public virtual GUIStyle Style => new GUIStyle("DD ItemStyle") {
			padding = {
				left = 30,
				right = 2,
				top = 2,
				bottom = 2
			},
			fixedHeight = EditorGUIUtility.singleLineHeight + 9,
			richText = true
		};

		private GUIStyle _iconStyle = new GUIStyle {
			padding = {
				left = 2
			},
			fixedHeight = EditorGUIUtility.singleLineHeight + 4,
		};

		public SimpleNode(GUIContent label) {
			Label = label;
			Children = new List<INode>();
		}

		public SimpleNode(string label): this(new GUIContent(label)) { }
		
		public SimpleNode(string label, Texture texture): this(new GUIContent(label, texture)) { }
		
		public virtual void Draw(Rect rect, bool isHovering, WindowStyle styles) {
			Style.Draw(rect, new GUIContent(Label.text), false, false, isHovering, isHovering);

			Rect iconTexture = rect;
			iconTexture.width = 32;
			iconTexture.x += 2;
			iconTexture.y += 2;
			iconTexture.width -= 4;
			iconTexture.height -= 4;
			GUI.Box(iconTexture, new GUIContent(Label.image), _iconStyle);

			if (Children.Count > 0) {
				// draw arrow
				Rect arrowRect = iconTexture;
				float fixedWidth = styles.RightArrowStyle.fixedWidth;
				arrowRect.x = rect.width - fixedWidth - 3;
				arrowRect.y += 3;
				arrowRect.width = fixedWidth;
							
				styles.RightArrowStyle.Draw(arrowRect, false, false, false, false);
			}
		}

		public virtual void OnClicked() { }
	}
}