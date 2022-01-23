using System;
using System.Collections.Generic;
using System.Linq;
using Nomnom.QuickScene.Editor.Tree.Nodes;

namespace Nomnom.QuickScene.Editor.Tree {
	/// <summary>
	/// Houses a collection of 
	/// </summary>
	public class NodeTree {
		private INode _rootNode;
		private List<INode> _quickLookup;
		private uint _idTracker;
		
		public NodeTree() {
			_rootNode = new SimpleNode("Root");
			_rootNode.Id = _idTracker++;
			_quickLookup = new List<INode>();
		}

		public NodeTree(INode rootNode) {
			_rootNode = rootNode;
			_rootNode.Id = _idTracker++;
			_quickLookup = new List<INode>();
		}

		/// <summary>
		/// Inserts a node under the root node
		/// </summary>
		public uint Insert(INode node) {
			if (node.Id == 0) {
				node.Id = _idTracker++;
			}
			
			_rootNode.Children.Add(node);
			_quickLookup.Add(node);

			return node.Id;
		}

		/// <summary>
		/// Inserts a node under a given existing parent
		/// </summary>
		public uint Insert(uint parent, INode node) {
			INode parentNode = Find(parent);

			if (parentNode == null) {
				throw new Exception($"[NodeTree] Parent ({parent}) not found!");
			}

			if (node.Id == 0) {
				node.Id = _idTracker++;
			}
			
			parentNode.Children.Add(node);
			_quickLookup.Add(node);

			return node.Id;
		}

		/// <summary>
		/// Removes all occurrences of a node
		/// </summary>
		public void Remove(uint id) {
			// remove node itself
			internalFind(_rootNode, id);
			
			bool internalFind(INode node, uint id) {
				if (node == null) {
					return false;
				}

				if (node.Id == id) {
					_quickLookup.Remove(node);
					return true;
				}
				
				// search children
				for (int i = 0; i < node.Children.Count; i++) {
					INode nodeChild = node.Children[i];
					
					if (!internalFind(nodeChild, id)) {
						continue;
					}

					_quickLookup.Remove(nodeChild);
					node.Children.RemoveAt(i--);

					return true;
				}

				return false;
			}
		}

		public INode Find(uint id) {
			return internalFind(_rootNode, id);
			
			INode internalFind(INode node, uint id) {
				if (node == null) {
					return null;
				}

				if (node.Id == id) {
					return node;
				}
				
				// search children
				foreach (INode nodeChild in node.Children) {
					INode output = internalFind(nodeChild, id);
					if (output == null) {
						continue;
					}

					return output;
				}

				return null;
			}
		}

		public List<INode> Find(Func<INode, bool> searchFunc) {
			return _quickLookup.Where(searchFunc).ToList();
		}

		public void Sort(Comparison<INode> func = null) {
			func ??= (a, b) => {
				int index = string.Compare(a.Label.text, b.Label.text);
				// check children status
				int aCount = a.Children.Count;
				int bCount = b.Children.Count;
				int childrenIndex =
					aCount > 0 && bCount > 0 ? index :
					aCount == 0 && bCount > 0 ? 1 :
					aCount > 0 && bCount == 0 ? -1 :
					index;

				return childrenIndex;
			};
			
			internalSort(_rootNode);
			
			void internalSort(INode node) {
				if (node == null) {
					return;
				}
				
				node.Children.Sort(func);

				// search children
				for (int i = 0; i < node.Children.Count; i++) {
					INode nodeChild = node.Children[i];
					internalSort(nodeChild);
				}
			}
		}
	}
}