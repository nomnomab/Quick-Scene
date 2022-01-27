using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Nomnom.QuickScene.Editor {
	public class QuickSceneInput {
		private static QuickSceneInput _instance;
		
		// keys
		public bool ShowPointer { get; private set; }
		public bool ShowOrigin { get; private set; }
		public bool F2 { get; private set; }
		public bool Del { get; private set; }

		// mouse boi
		public bool M0 { get; private set; }
		public bool M1 { get; private set; }

		private List<Key> _pointerKeys;
		private List<Key> _originKeys;
		
		private ShortcutBinding _pointerShortcut;
		private ShortcutBinding _originShortcut;

		public QuickSceneInput() {
			_instance = this;
			_pointerKeys = new List<Key>();
			_originKeys = new List<Key>();

			RefreshKeybinds();
		}
		
		public void Update(Event e) {
			ValidateKey(e);
			ValidateMouse(e);
		}

		private void ValidateKey(Event e) {
			bool isKeyDown = e.type == EventType.KeyDown;

			bool keyCombinationGood = true;
			foreach (Key key in _pointerKeys) {
				bool gotOne = key.Pressed;
				
				foreach (KeyCode pointerKey in key.Keys) {
					if (e.keyCode == pointerKey) {
						gotOne = isKeyDown;
						break;
					}
				}

				key.Pressed = gotOne;

				if (!gotOne) {
					keyCombinationGood = false;
				}
			}

			ShowPointer = keyCombinationGood;

			keyCombinationGood = true;
			foreach (Key key in _originKeys) {
				bool gotOne = key.Pressed;
				foreach (KeyCode pointerKey in key.Keys) {
					if (e.keyCode == pointerKey) {
						gotOne = isKeyDown;
						break;
					}
				}

				key.Pressed = gotOne;

				if (!gotOne) {
					keyCombinationGood = false;
				}
			}
			
			ShowOrigin = keyCombinationGood;
			
			switch (e.keyCode) {
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
			ShowPointer = ShowOrigin = F2 = M0 = M1 = false;
			
			foreach (Key key in _pointerKeys) {
				key.Pressed = false;
			}
			
			foreach (Key key in _originKeys) {
				key.Pressed = false;
			}
		}
		
		[MenuItem("Edit/Quick Scene/Add - Pointer #a")]
		private static void ShowAddPointer() { }
		
		[MenuItem("Edit/Quick Scene/Add - Origin #&a")]
		private static void ShowAddPointerOrigin() { }

		[MenuItem("Edit/Quick Scene/Refresh Keybinds")]
		private static void RefreshKeybinds() {
			_instance._pointerShortcut = ShortcutManager.instance.GetShortcutBinding("Main Menu/Edit/Quick Scene/Add - Pointer");
			_instance._originShortcut = ShortcutManager.instance.GetShortcutBinding("Main Menu/Edit/Quick Scene/Add - Origin");
			
			generateKeys(_instance._pointerKeys, _instance._pointerShortcut);
			generateKeys(_instance._originKeys, _instance._originShortcut);

			void generateKeys(List<Key> keys, ShortcutBinding binding) {
				keys.Clear();
				
				foreach (KeyCombination keyCombination in binding.keyCombinationSequence) {
					keys.Add(new Key(keyCombination.keyCode));
					if (keyCombination.alt) {
						keys.Add(new Key(KeyCode.LeftAlt, KeyCode.RightAlt));
					}

					if (keyCombination.shift) {
						keys.Add(new Key(KeyCode.LeftShift, KeyCode.RightShift));
					}

					if (keyCombination.action) {
						keys.Add(new Key(KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftCommand, KeyCode.RightCommand));
					}
				}
			}
		}

		private class Key {
			public KeyCode[] Keys;
			public bool Pressed;

			public Key(params KeyCode[] keys) {
				Keys = keys;
			}
		}
	}
}