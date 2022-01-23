using UnityEngine;

namespace Nomnom.QuickScene.Editor {
	public class QuickSceneInput {
		// keys
		public bool Shift { get; private set; }
		public bool Ctrl { get; private set; }
		public bool Alt { get; private set; }
		public bool F2 { get; private set; }
		public bool Del { get; private set; }
		
		// mouse boi
		public bool M0 { get; private set; }
		public bool M1 { get; private set; }
		
		public void Update(Event e) {
			ValidateKey(e);
			ValidateMouse(e);
		}

		private void ValidateKey(Event e) {
			bool isKeyDown = e.type == EventType.KeyDown;
			
			switch (e.keyCode) {
				case KeyCode.LeftShift:
				case KeyCode.RightShift:
					Shift = isKeyDown;
					break;
				case KeyCode.LeftControl:
				case KeyCode.RightControl:
					Ctrl = isKeyDown;
					break;
				case KeyCode.LeftAlt:
				case KeyCode.RightAlt:
					Alt = isKeyDown;
					break;
				case KeyCode.F2:
					F2 = isKeyDown;
					break;
				case KeyCode.Delete:
					Del = isKeyDown;
					break;
			}
		}

		private void ValidateMouse(Event e) {
			bool isMouseDown = e.type == EventType.MouseDown;

			switch (e.button) {
				case 0:
					M0 = isMouseDown;
					break;
				case 1:
					M1 = isMouseDown;
					break;
			}
		}

		public void Reset() {
			Ctrl = Shift = Alt = F2 = M0 = M1 = false;
		}
	}
}