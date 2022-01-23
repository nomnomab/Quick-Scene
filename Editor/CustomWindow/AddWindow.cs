using System;
using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using FuzzySharp.PreProcess;
using Nomnom.QuickScene.Editor.Tree;
using Nomnom.QuickScene.Editor.Tree.Nodes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.CustomWindow {
	internal class AddWindow: QuickWindow<AddWindow>, IDisposable {
		private const string KEY_USE_NORMAL = "QuickScene-use_normal";
		private const string KEY_USE_PARENTING = "QuickScene-use_parenting";
		
		private static GUIStyle _iconStyle;
		private WindowState _windowState;
		private SearchField _searchField;
		private string _searchString;
		private Vector2 _scrollState;
		private Texture _normalTexture;
		private Texture _parentTexture;
		private bool _isUsingNormal;
		private bool _isUsingTargetParent;

		public static AddWindow Open(WindowState state, Vector2 screenCoords, Vector2 size) {
			Open(screenCoords, size);
			
			Instance._windowState = state;
			Instance._searchField = new SearchField();
			Instance._searchField.SetFocus();

			return Instance;
		}
		
		public override void Dispose() {
			EditorPrefs.SetBool(KEY_USE_NORMAL, _isUsingNormal);
			EditorPrefs.SetBool(KEY_USE_PARENTING, _isUsingTargetParent);

			_windowState = null;
			_searchField = null;
		}

		public override void Init() {
			_isUsingNormal = EditorPrefs.GetBool(KEY_USE_NORMAL, false);
			_isUsingTargetParent = EditorPrefs.GetBool(KEY_USE_PARENTING, false);

			_normalTexture = Resources.Load<Texture>("QuickScene/d_normal");
			_parentTexture = new EditorIcon("ParentConstraint Icon").Get().image;
			_iconStyle ??= new GUIStyle {
				fixedHeight = 30,
				fixedWidth = 30,
				padding = {
					top = 2,
					left = 2,
					right = 2,
					bottom = 4
				}
			};
		}

		public override void ShowAs(Rect rect, Vector2 size) {
			// show as a dropdown
			ShowAsDropDown(rect, size);
		}

		public override bool OnPreGUI(Event e) {
			return _windowState != null;
		}

		public override void OnDrawGUI(Event e) {
			DrawSearchBar();
			DrawHeader(e);
			DrawTree(e);
			DrawFooter(e);

			Vector2 screenCoords = GUIUtility.GUIToScreenPoint(e.mousePosition);

			if (position.Contains(screenCoords)) {
				Repaint();
			}
		}

		private void DrawSearchBar() {
			GUILayout.Space(6);
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(4);
				string newStr = _searchField.OnGUI(_searchString);

				if (newStr != _searchString) {
					// fire event
					OnSearchChanged(_searchString, newStr);
					
					_searchString = newStr;
				}
				
				GUILayout.Space(2);
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(2);
		}
		
		private void DrawHeader(Event e) {
			bool hasHistory = _windowState.History.Count > 0 && string.IsNullOrEmpty(_searchString);
			
			// draw subtitle overhang
			EditorGUILayout.BeginHorizontal();
			{
				GUIContent content = new GUIContent(_windowState.CurrentNode.Label.text);
				Rect headerRect =
					GUILayoutUtility.GetRect(content, _windowState.Style.HeaderStyle, GUILayout.ExpandWidth(true));
				// validate hover action
				bool isHoveringHeader = hasHistory && headerRect.Contains(e.mousePosition);

				if (e.type == EventType.Repaint) {
					_windowState.Style.HeaderStyle.Draw(headerRect, _windowState.CurrentNode.Label, isHoveringHeader, false, false, false);
				}

				// if needed, draw a back button
				if (hasHistory) {
					// button for handling the click event
					if (GUI.Button(headerRect, GUIContent.none, GUIStyle.none)) {
						_windowState.CurrentNode = _windowState.History.Pop();
					}
					
					float fixedArrowWidth = _windowState.Style.LeftArrowStyle.fixedWidth;
					Rect leftArrowRect = new Rect(
						headerRect.x + _windowState.Style.LeftArrowStyle.margin.left,
						headerRect.y + (headerRect.height - fixedArrowWidth) * 0.5f,
						fixedArrowWidth,
						fixedArrowWidth
					);

					if (e.type == EventType.Repaint) {
						_windowState.Style.LeftArrowStyle.Draw(leftArrowRect, false, false, false, false);
					}
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawTree(Event e) {
			if (_windowState.CurrentNode == null) {
				return;
			}
			
			_scrollState = EditorGUILayout.BeginScrollView(_scrollState);
			{
				// get children
				List<INode> children = _windowState.CurrentNode.Children;
				foreach (INode child in children) {
					GUIContent labelContent = new GUIContent(child.Label.text);
					Rect itemRect = GUILayoutUtility.GetRect(labelContent, child.Style, GUILayout.ExpandWidth(true));
					bool canSelect = child.CanSelect;
					bool isHovering = canSelect && itemRect.Contains(e.mousePosition) && !PointInFooter(e.mousePosition);

					if (e.type == EventType.Repaint) {
						child.Draw(itemRect, isHovering, _windowState.Style);
					}
					
					// validate input
					if (canSelect && GUI.Button(itemRect, GUIContent.none, GUIStyle.none)) {
						child.OnClicked();

						if (child.Children.Count > 0) {
							// we have a parent
							_windowState.History.Push(_windowState.CurrentNode);
							_windowState.CurrentNode = child;
						} else {
							OnClose();
						}
					}
				}
			}
			EditorGUILayout.EndScrollView();
		}

		private void DrawFooter(Event e) {
			EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(31));
			{
				if (GUILayout.Button(new GUIContent(_normalTexture, "Align to Normal"), _iconStyle, GUILayout.Width(31), GUILayout.MaxHeight(31))) {
					_isUsingNormal = !_isUsingNormal;
				}

				if (e.type == EventType.Repaint) {
					Rect normalRect = GUILayoutUtility.GetLastRect();
					EditorGUI.DrawRect(new Rect(normalRect.position, new Vector2(position.width, 1)), Color.black * 0.4f);
					if (normalRect.Contains(e.mousePosition) || _isUsingNormal) {
						EditorGUI.DrawRect(normalRect, Color.black * 0.2f);
					}
				}
				
				GUILayout.Space(1);

				if (_windowState?.Surface) {
					if (GUILayout.Button(new GUIContent(_parentTexture, "Parent to Surface"), _iconStyle, GUILayout.Width(31),
						GUILayout.MaxHeight(31))) {
						_isUsingTargetParent = !_isUsingTargetParent;
					}

					if (e.type == EventType.Repaint) {
						Rect normalRect = GUILayoutUtility.GetLastRect();
						if (normalRect.Contains(e.mousePosition) || _isUsingTargetParent) {
							EditorGUI.DrawRect(normalRect, Color.black * 0.2f);
						}
					}
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		
		private void OnSearchChanged(string oldStr, string newStr) {
			if (string.IsNullOrEmpty(newStr)) {
				// revert back to previous tree
				_windowState.CurrentNode = _windowState.History.Pop();
				return;
			}
			
			// check if oldStr is root
			if (string.IsNullOrEmpty(oldStr)) {
				// stash root into history
				_windowState.History.Push(_windowState.CurrentNode);
				_windowState.CurrentNode = null;
			}
			
			// create new root
			_windowState.CurrentNode ??= new SimpleNode("Search Results");
			INode searchNode = _windowState.CurrentNode;
			searchNode.Children.Clear();
			
			// collect scores
			(INode n, int score)[] nodes = _windowState.Tree.Find(n => n.Children.Count == 0 && n.CanBeSearched)
				.Select(n => {
					int score = Fuzz.PartialTokenSetRatio(n.Label.text, newStr, PreprocessMode.Full);
					if (n.Label.text.IndexOf(newStr, StringComparison.OrdinalIgnoreCase) != -1) {
						score += 50;
					}
					return (n, score);
				})
				.OrderByDescending(n => n.score)
				.ToArray();

			int average = nodes.Sum(n => n.score) / nodes.Length - nodes.Length;

			foreach ((INode n, int score) in nodes) {
				//Debug.Log($"[{score}] {n.Label.text}");
				if (score < average) {
					continue;
				}
				
				searchNode.Children.Add(n);
			}
		}

		public static void PlaceObject(GameObject obj) {
			obj.transform.position = Instance._windowState.HitPoint;

			if (Instance._isUsingNormal) {
				obj.transform.up = Instance._windowState.HitNormal;
			}

			if (Instance._isUsingTargetParent) {
				obj.transform.SetParent(Instance._windowState.Surface, true);
			}
		}

		private bool PointInFooter(Vector2 point) {
			return new Rect(position.x, position.y + position.height - 31, position.width, 31).Contains(point);
		}
	}
}