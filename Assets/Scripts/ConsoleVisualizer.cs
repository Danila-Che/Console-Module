using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ConsoleModule {
	class ConsoleVisualizer {
		private GameObject _console;
		private Text _text;
		private int _maxCountOfMessages;

		public ConsoleVisualizer (GameObject console, Text text, int maxCountOfMessages) {
			_console = console;
			_text = text;
			_maxCountOfMessages = maxCountOfMessages;
		}

		public void WriteMessagesInConsole (List<Message> messages) {
			_text.text = "";
			int downBorderIndex = messages.Count > _maxCountOfMessages ? messages.Count - _maxCountOfMessages : 0;
			int maxLength = messages.Count > _maxCountOfMessages ? _maxCountOfMessages : messages.Count;

			for (int i = downBorderIndex; i < downBorderIndex + maxLength; i++) {
				Message message = messages [i];
				_text.text += $"<color=#{ColorUtility.ToHtmlStringRGBA (message.Color)}>[{message.Time.ToLongTimeString ()}] {message.Text}</color>\n";
			}
		}

		public void RemoveAllMessageFromConsole () {
			_text.text = "";
		}

		public void ShowConsole () {
			_console.SetActive (true);
		}

		public void HideConsole () {
			_console.SetActive (false);
		}
	}
}
