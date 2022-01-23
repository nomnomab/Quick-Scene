using Nomnom.QuickScene.Editor.CustomWindow;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Tree.Nodes {
	public class SpawnNode: SimpleNode {
		public readonly string AssetPath;

		public SpawnNode(string assetPath, GUIContent label) : base(label) {
			AssetPath = assetPath;
		}

		public SpawnNode(string assetPath, string label) : this(assetPath, new GUIContent(label)) { }
		
		public SpawnNode(string assetPath, string label, Texture texture) : this(assetPath, new GUIContent(label, texture)) { }

		public override void OnClicked() {
			base.OnClicked();

			GameObject instance = null;
			
			// check if this is a primative
			if (AssetPath.StartsWith("p:")) {
				string str = AssetPath.Substring(2);

				switch (str) {
					case "Cube": instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
						break;
					case "Sphere": instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						break;
					case "Cylinder": instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
						break;
					case "Capsule": instance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
						break;
					case "Plane": instance = GameObject.CreatePrimitive(PrimitiveType.Plane);
						break;
					case "Quad": instance = GameObject.CreatePrimitive(PrimitiveType.Quad);
						break;
				}
			} else {
				instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetPath));
			}
			
			// send to the window for finishing
			AddWindow.PlaceObject(instance);

			// NamingWindow.InUse = true;
			Selection.activeGameObject = instance;
		}
	}
}