using UnityEditor;
using UnityEngine;

namespace Nomnom.QuickScene.Editor {
	public class ScenePlane {
		public bool IsValid { get; private set; }
		public Vector3 Point { get; private set; }
		public Vector3 Normal { get; private set; }
		public Transform Surface { get; private set; }

		public Vector3 Origin { get; private set; }

		public void Update(Event e, SceneView sceneView) {
			// check object state
			Vector2 mousePos = e.mousePosition;
			Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

			IsValid = Physics.Raycast(ray, out RaycastHit hit);
			Transform transform = sceneView.camera.transform;
			
			Vector3 pivot = sceneView.pivot;
			Vector3 up = transform.up;
			Vector3 scenePoint = transform.position;
			Vector3 forward = transform.forward;
			
			float dist = Vector3.Distance(scenePoint, pivot);
			
			Vector3 frontPoint = scenePoint + forward * dist;
			Vector3 backPoint = scenePoint - up * dist;
			Vector3 midPoint = (frontPoint + backPoint) * 0.5f;
			Vector3 dir = (scenePoint - midPoint).normalized;
			
			Plane plane = new Plane(dir, pivot);

			if (!IsValid && plane.Raycast(ray, out float enter)) {
				IsValid = true;
				hit.point = ray.GetPoint(enter);
			}

			Point = hit.point;
			Origin = ray.origin;
			Surface = hit.transform;
			Normal = hit.normal;
		}
	}
}