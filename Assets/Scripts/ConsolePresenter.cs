using UnityEngine;
using UnityEngine.UI;

namespace ConsoleModule {
	class ConsolePresenter: ConsoleMessageHandler {
		[Header ("Console Canvas Data")]
		[SerializeField] private GameObject _consoleCanvas;
		[SerializeField] private Text _textOfMessage;
		[SerializeField] private InputField _comandConsoleField;

        private ConsoleVisualizer _consoleVisualizer;
        private CommandLine _commandLine;
        private ConsoleOpenInput _input;

        private void Awake () {
            _consoleVisualizer = new ConsoleVisualizer (_consoleCanvas, _textOfMessage, _maxCountOfMassage);
            _commandLine = new CommandLine (_consoleVisualizer, messages: this, listOfMessage: this);
            _input = new ConsoleOpenInput (KeyCode.LeftControl, KeyCode.C);

            _consoleVisualizer.WriteMessagesInConsole (_messagesShowedOnlyInConsole);
        }

		private void Update () {
            if (_input.HasKeyPressed ()) {
                ShowConsole ();
            }
        }

		public void ShowConsole () {
            _consoleVisualizer.ShowConsole ();
        }

        public void CloseConsole () {
            _consoleVisualizer.HideConsole ();
        }

        public void EnterCommand () {
            string command = _comandConsoleField.text;
            if (_commandLine.HasCommand (command)) {
                _commandLine.EnterCommand (command);

                WriteCommnadMessage (AddNotification ($"введена команда \"{command}\""));
            } else {
                WriteCommnadMessage (AddError ($"введена некорректная команда \"{command}\""));
            }

            _comandConsoleField.text = string.Empty;
        }

        private void WriteCommnadMessage (MessageEventSignature messageEvent) {
            TriggerEvent (messageEvent);
            StopListening (messageEvent);
        }

        protected override void UpdateConsoleOutput () {
            _consoleVisualizer.WriteMessagesInConsole (_messagesShowedOnlyInConsole);
        }
    }
}
