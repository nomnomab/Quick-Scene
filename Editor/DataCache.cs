using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Nomnom.QuickScene.Editor.CustomWindow;
using Nomnom.QuickScene.Editor.Tree;
using Nomnom.QuickScene.Editor.Tree.Nodes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nomnom.QuickScene.Editor {
	/// <summary>
	/// Holds all of the cached types and information for quick usage
	/// </summary>
	internal static class DataCache {
		public static IEnumerable<string> Prefabs { get; private set; }
		public static NodeTree DefaultTree { get; private set; }
		// public static Dictionary<string, Type> ShortFormToType;

		public static Texture FolderIcon => GetIcon("Folder Icon");
		public static Texture AddIcon => GetIcon("r:add_cross");
		public static Texture SpriteIcon => GetIcon("SpriteAtlasAsset Icon");
		public static Texture RigidbodyIcon => GetIcon("Rigidbody Icon");
		
		private static Dictionary<string, EditorIcon> _icons { get; set; }
		private static Dictionary<Type, Texture> _typeIcons { get; set; }

		private static string _manifestText;
		private static bool _has2DSpritePackage;
		private static bool _has2DSpriteShapePackage;

		[InitializeOnLoadMethod]
		private static void OnLoad() {
			_icons = new Dictionary<string, EditorIcon>();
			_typeIcons = new Dictionary<Type, Texture>();

			FindPrefabs();

			EditorApplication.projectChanged -= OnLoad;
			EditorApplication.projectChanged += OnLoad;
		}

		// private static void CheckManifest() {
		// 	_has2DSpritePackage = false;
		// 	_has2DSpriteShapePackage = false;
		// 	
		// 	if (!File.Exists("Packages/manifest.json")) {
		// 		return;
		// 	}
		// 	
		// 	_manifestText = File.ReadAllText("Packages/manifest.json");
		// 	_has2DSpritePackage = _manifestText.Contains("com.unity.2d.sprite");
		// 	_has2DSpriteShapePackage = _manifestText.Contains("com.unity.2d.spriteshape");
		// }

		public static void FindPrefabs() {
			Prefabs = AssetDatabase.FindAssets("t:prefab")
				.Select(AssetDatabase.GUIDToAssetPath);

			DefaultTree = new NodeTree(new SimpleNode("Quick Scene"));

			// get all method infos
			Dictionary<string, INode> groups = new Dictionary<string, INode>();
			INode root = DefaultTree.Find(0);
			groups[string.Empty] = root;
			StringBuilder sb = new StringBuilder();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type type in assembly.GetTypes()) {
					MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
					foreach (MethodInfo methodInfo in methods) {
						var items = methodInfo.GetCustomAttributes<MenuItem>();

						if (items == null) {
							continue;
						}

						foreach (MenuItem menuItem in items) {
							if (!menuItem.menuItem.TrimStart().StartsWith("GameObject")) {
								continue;
							}
							
							string[] split = menuItem.menuItem.Split('/');

							INode lastNode = root;

							sb.Clear();
							for (int i = 1; i < split.Length - 1; i++) {
								string cur = sb.ToString();
								string str = split[i];
								string tmp = $"{cur}/{str}";

								if (!groups.TryGetValue(tmp, out INode curFolder)) {
									SimpleNode folder = new SimpleNode(str, FolderIcon);
									DefaultTree.Insert(lastNode.Id, folder);
									groups[tmp] = curFolder = folder;
								}

								if (i > 0) {
									sb.Append('/');
								}
							
								sb.Append(str);

								lastNode = curFolder;
							}

							string name = Path.GetFileName(menuItem.menuItem);
							Texture texture = EditorIcon.FromType(name);

							if (!texture) {
								texture = AddIcon;
							}

							if (lastNode.Children.Any(n => n.Label.text == name)) {
								continue;
							}
						
							DefaultTree.Insert(lastNode.Id, new MethodNode(() => {
								EditorApplication.ExecuteMenuItem(menuItem.menuItem);
								GameObject selection = Selection.activeGameObject;
								// NamingWindow.InUse = true;
							
								if (selection) {
									AddWindow.PlaceObject(selection);
								}
							
								// QuickSceneTool.onSceneFrameDelay += () => NamingWindow.Init(true, true, true);
							}, name, texture));
						}
					}
				}
			}
			
			// filter root node's clutter
			// only take Camera and Create Empty
			List<INode> toRemove = new List<INode>();
			for (int i = 0; i < root.Children.Count; i++) {
				INode child = root.Children[i];

				if (child.Children.Count > 0 || !child.CanBeSearched || !child.CanSelect) {
					continue;
				}

				string trimmed = child.Label.text;
				int lastSpace = trimmed.LastIndexOf(' ');
				trimmed = lastSpace == -1 ? trimmed : trimmed.Substring(0, lastSpace);

				if (trimmed == "Create Empty" || trimmed == "Camera") {
					child.Label.text = trimmed;
					continue;
				}
				
				toRemove.Add(child);
			}

			foreach (INode node in toRemove) {
				DefaultTree.Remove(node.Id);
			}
			
			DefaultTree.Sort();
			
			// apply 3D Object groups
			List<INode> results = DefaultTree.Find(n => n.Children.Count > 0 && n.Label.text == "3D Object");
			INode threeDNode = results[0];
			
			// sort this node with primitives at the top
			HeaderNode primitiveHeader = new HeaderNode("Primitives");
			// do a smart sort here to get the primitives on top
			Texture gameObjectIcon = EditorIcon.AdditionalIconSwaps["Cube"].Get().image;
			DefaultTree.Sort((a, b) => {
				bool isA = a.Label.image == gameObjectIcon;
				bool isB = b.Label.image == gameObjectIcon;

				return isA && !isB ? -1 : isB && !isA ? 1 : 0;
			});

			DefaultTree.Insert(threeDNode.Id, primitiveHeader);
			// move higher in the list
			threeDNode.Children.RemoveAt(threeDNode.Children.Count - 1);
			threeDNode.Children.Insert(0, primitiveHeader);
			
			// get count of primitives
			int primitiveCount = threeDNode.Children.Count(n => n.Label.image == gameObjectIcon);
			
			// 'other' header
			HeaderNode otherHeader = new HeaderNode("Other");
			DefaultTree.Insert(threeDNode.Id, otherHeader);
			// move to below the primitives
			threeDNode.Children.RemoveAt(threeDNode.Children.Count - 1);
			threeDNode.Children.Insert(primitiveCount + 1, otherHeader);
			
			HeaderNode createHeader = new HeaderNode("Create");
			DefaultTree.Insert(createHeader);
			// move higher in the list
			root.Children.RemoveAt(root.Children.Count - 1);
			root.Children.Insert(0, createHeader);

			DefaultTree.Insert(new SpacerNode(16));
			
			// modify 2D things if those exist
			results = DefaultTree.Find(n => n.Label.text == "2D Object");
			if (results.Count > 0) {
				// package is valid
				INode twoDNode = results[0];
				// check for a Sprites item
				INode spritesNode = twoDNode.Children.FirstOrDefault(n => n.Label.text == "Sprites");

				if (spritesNode != null) {
					// swap all icons to sprite icons
					foreach (INode spritesNodeChild in spritesNode.Children) {
						spritesNodeChild.Label.image = GetIcon("Sprite Icon");
					}
				}

				INode spriteShapes = twoDNode.Children.FirstOrDefault(n => n.Label.text == "Physics");

				if (spriteShapes != null) {
					foreach (INode spriteShapesChild in spriteShapes.Children) {
						spriteShapesChild.Label.image = GetIcon("Rigidbody Icon");
					}
				}
			}

			// add prefabs
			HeaderNode prefabHeader = new HeaderNode("Prefabs");
			DefaultTree.Insert(prefabHeader);

			groups.Clear();
			groups[string.Empty] = root;
			sb.Clear();

			foreach (string assetPath in Prefabs) {
				string[] split = assetPath.Split('/');

				INode lastNode = root;

				sb.Clear();
				for (int i = 1; i < split.Length - 1; i++) {
					string cur = sb.ToString();
					string str = split[i];
					string tmp = $"{cur}/{str}";

					if (!groups.TryGetValue(tmp, out INode curFolder)) {
						SimpleNode folder = new SimpleNode(str, FolderIcon);
						DefaultTree.Insert(lastNode.Id, folder);
						groups[tmp] = curFolder = folder;
					}

					if (i > 0) {
						sb.Append('/');
					}
							
					sb.Append(str);

					lastNode = curFolder;
				}

				string name = Path.GetFileName(assetPath);

				if (lastNode.Children.Any(n => n.Label.text == name)) {
					continue;
				}
						
				// get texture of prefab
				AssetNode assetNode = new AssetNode(assetPath, name, null);
				string tmpAssetPath = assetPath;
				DefaultTree.Insert(lastNode.Id, assetNode);

				EditorApplication.update += waitForAssetPreview;

				void waitForAssetPreview() {
					if (AssetPreview.IsLoadingAssetPreviews()) {
						return;
					}
					
					Texture2D preview = AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath<Object>(tmpAssetPath));

					if (!preview) {
						preview = (Texture2D)gameObjectIcon;
					}
					assetNode.Label.image = preview;
					
					EditorApplication.update -= waitForAssetPreview;
				}
			}
			
			// sort prefabs into folder -> item order
			int indexOfPrefabHeader = root.Children.IndexOf(prefabHeader);
			List<INode> subset = new List<INode>();
			for (int i = indexOfPrefabHeader + 1; i < root.Children.Count; i++) {
				subset.Add(root.Children[i]);
			}
			
			// first sort by folder / child
			subset.Sort((a, b) => {
				int index = string.CompareOrdinal(a.Label.text, b.Label.text);
				// check children status
				int aCount = a.Children.Count;
				int bCount = b.Children.Count;
				int childrenIndex =
					aCount > 0 && bCount > 0 ? index :
					aCount == 0 && bCount > 0 ? 1 :
					aCount > 0 && bCount == 0 ? -1 :
					index; // none of the above, use prior score

				return childrenIndex;
			});

			// folder subset
			List<INode> folderSubset = new List<INode>();
			foreach (INode node in subset) {
				if (node.Children.Count == 0) {
					break;
				}
				
				folderSubset.Add(node);
			}
			
			// sort
			folderSubset.Sort((a, b) => string.Compare(a.Label.text, b.Label.text));
			// insert back
			for (int i = 0; i < folderSubset.Count; i++) {
				subset[i] = folderSubset[i];
			}

			// reinsert the list
			for (int i = indexOfPrefabHeader + 1, j = 0; i < root.Children.Count; i++, j++) {
				root.Children[i] = subset[j];
			}
		}

		public static Texture GetIcon(string name) {
			if (!_icons.TryGetValue(name, out EditorIcon texture)) {
				_icons[name] = texture = new EditorIcon(name);
			}

			return texture.Get().image;
		}

		public static Texture GetIconType(Object obj) {
			if (!_typeIcons.TryGetValue(obj.GetType(), out Texture texture)) {
				_typeIcons[obj.GetType()] = texture = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image;
			}

			return texture;
		}
	}
}