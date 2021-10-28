using System.IO;
using System.Collections.Generic;
using ConsoleModule.Commands;

namespace ConsoleModule {
	class CommandLine {
		private readonly ConsoleVisualizer _consoleVisualizer;
		private readonly IMessages _messages;
		private readonly ISavedMessage _listOfSavedMessage;

		private List<ICommand> _commands = new List<ICommand> ();

		public CommandLine (ConsoleVisualizer consoleVisualizer, IMessages messages, ISavedMessage listOfMessage) {
			_consoleVisualizer = consoleVisualizer;
			_messages = messages;
			_listOfSavedMessage = listOfMessage;

			_commands.Add (new ClearMessage (consoleVisualizer, messages));
			_commands.Add (new SaveMessage (listOfMessage));
		}

		public bool HasLastCommnadFinished { get; private set; }

		public bool HasCommand (string commandText) {
			foreach (ICommand command in _commands) {
				if (command.NameCommand == commandText)
					return true;
			}

			return false;
		}

		public void EnterCommand (string commandText) {
			foreach (ICommand command in _commands) {
				if (command.NameCommand == commandText)
					command.Invoke ();
			}
		}
	}

	namespace Commands {
		interface IInvoker {
			public void Invoke ();
		}

		interface ICommand: IInvoker {
			public string NameCommand { get; }
		}
		
		class ClearMessage: ICommand {
			private readonly ConsoleVisualizer _consoleVisualizer;
			private readonly IMessages _messages;

			public ClearMessage (ConsoleVisualizer consoleVisualizer, IMessages messages) {
				_consoleVisualizer = consoleVisualizer;
				_messages = messages;
			}

			public string NameCommand { get; } = "clear";

			public void Invoke () {
				_messages.ClearMessages ();
				_consoleVisualizer.RemoveAllMessageFromConsole ();
			}
		}

		class SaveMessage: ICommand {
			private readonly ISavedMessage _listOfSavedMessage;

			public SaveMessage (ISavedMessage listOfSavedMessage) {
				_listOfSavedMessage = listOfSavedMessage;
			}

			public string NameCommand { get; } = "save";

			public void Invoke () {
				SaveDate ();
			}

			private void SaveDate () {
				using StreamWriter file = new StreamWriter (@"C:\temp\Logs.txt");

				foreach (Message message in _listOfSavedMessage.SavedMessages) {
					file.WriteLine ($"Type: {message.LogsType} [{message.Time.ToLongTimeString ()}] {message.Text}");
				}
			}
		}
	}
}
