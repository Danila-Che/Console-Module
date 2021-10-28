using UnityEngine;

namespace ConsoleModule {
	class ConsoleOpenInput {

		private KeyCode [] _keyCodes;

		public ConsoleOpenInput (params KeyCode [] keyCodes) {
			_keyCodes = keyCodes;
		}

		public bool HasKeyPressed () {
			if (_keyCodes.Length == 0) {
				return true;
			} else if (_keyCodes.Length == 1) {
				return Input.GetKeyDown (_keyCodes [0]);
			} else {
				bool isPressed = true;

				for (int i = 0; i < _keyCodes.Length - 1; i++)
					isPressed &= Input.GetKey (_keyCodes [i]);

				isPressed &= Input.GetKeyDown (_keyCodes [_keyCodes.Length - 1]);

				return isPressed;
			}
		}
	}
}
