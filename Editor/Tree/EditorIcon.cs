using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor.Tree {
	public class EditorIcon {
		public static readonly Dictionary<string, EditorIcon> AdditionalIconSwaps = new Dictionary<string, EditorIcon> {
			{ "Line", new EditorIcon("LineRenderer Icon") },
			{ "Trail", new EditorIcon("TrailRenderer Icon") },
			{ "Cube", new EditorIcon("GameObject Icon") },
			{ "Sphere", new EditorIcon("GameObject Icon") },
			{ "Cylinder", new EditorIcon("GameObject Icon") },
			{ "Capsule", new EditorIcon("GameObject Icon") },
			{ "Plane", new EditorIcon("GameObject Icon") },
			{ "Quad", new EditorIcon("GameObject Icon") },
			{ "Tree", new EditorIcon("tree_icon") },
			{ "Ragdoll...", new EditorIcon("Rigidbody Icon") },
		};

		private readonly GUIContent _light;
		private readonly GUIContent _dark;

		public EditorIcon(string light) : this(light, $"d_{light}") { }
		
		public EditorIcon(string light, string dark) {
			if (light.StartsWith("r:")) {
				string substring = light.Substring(2);
				light = $"QuickScene/{substring}";
				dark = $"QuickScene/d_{substring}";

				_light = new GUIContent(Resources.Load<Texture>(light));
				_dark = new GUIContent(Resources.Load<Texture>(dark));

				return;
			}

			_light = EditorGUIUtility.IconContent(light);
			_dark = EditorGUIUtility.IconContent(dark);
		}

		public GUIContent Get() {
			return EditorGUIUtility.isProSkin ? _dark : _light;
		}

		public static Texture FromType(string name) {
			name = name.Replace(" ", string.Empty);

			string testName = $"{name} Icon";
			Debug.unityLogger.logEnabled = false;
			Texture result = EditorGUIUtility.IconContent(testName).image;
			Debug.unityLogger.logEnabled = true;

			if (result) {
				return result;
			}

			testName = $"{name} Gizmo";
			Debug.unityLogger.logEnabled = false;
			result = EditorGUIUtility.IconContent(testName).image;
			Debug.unityLogger.logEnabled = true;

			if (!result) {
				if (AdditionalIconSwaps.TryGetValue(name, out EditorIcon icon)) {
					result = icon.Get().image;
				} else {
					if (name.ToLower().Contains("text")) {
						result = DataCache.GetIcon("Text Icon");
					}
				}
			}

			return result;
		}
	}
}