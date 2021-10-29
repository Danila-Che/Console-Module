using System.Collections.Generic;
using System;
using UnityEngine;

namespace ConsoleModule {
    public class ConsoleMessageHandler: ConsoleHandleListener<ConsoleMessageHandler>, ISavedMessage, IMessages {
        [Header ("Console Setting")]
        [SerializeField] protected int _maxCountOfMassage;

        protected List<Message> _messagesShowedOnlyInConsole = new List<Message> ();
        private List<Message> _savedMessages = new List<Message> ();

        public List<Message> SavedMessages => _savedMessages;

        public void ClearMessages () {
            _messagesShowedOnlyInConsole.Clear ();
        }

        public static MessageEventSignature AddNotification (string message, bool log = true) {
            return Instance.AddMessage (message, Color.blue, LogsTypes.Notification, log);
        }

        public static MessageEventSignature AddWarning (string message, bool log = true) {
            return Instance.AddMessage (message, Color.yellow, LogsTypes.Warning, log);
        }

        public static MessageEventSignature AddError (string message, bool log = true) {
            return Instance.AddMessage (message, Color.red, LogsTypes.Error, log);
        }

        private MessageEventSignature AddMessage (string messageText, Color color, LogsTypes logType, bool log) {
            MessageEventSignature eventSignature = new MessageEventSignature (messageText);

            StartListening (eventSignature, () => {
                PutMessageInData (messageText, color, logType, log);
                UpdateConsoleOutput ();
            });

            return eventSignature;
        }

        private void PutMessageInData (string messageText, Color color, LogsTypes logType, bool log) {
            Message message = new Message (messageText, GetNowTime (), color, logType);
            _messagesShowedOnlyInConsole.Add (message);

            if (log)
                _savedMessages.Add (message);
        }

        private DateTime GetNowTime () {
            return DateTime.Now;
        }

        protected virtual void UpdateConsoleOutput () { }
    }
}
