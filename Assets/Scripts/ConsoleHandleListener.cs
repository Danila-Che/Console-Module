using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace ConsoleModule {
    public abstract class ConsoleHandleListener<T>: MonoBehaviour where T: ConsoleHandleListener<T> {
        private Dictionary<MessageEventSignature, UnityEvent> eventDictionary;

        private static T _eventListner;
        private readonly static object consoleLock = new object ();

        public static T Instance {
            get {
                lock (consoleLock) {
                    if (!_eventListner) {
                        _eventListner = FindObjectOfType (typeof (T)) as T;

                        if (!_eventListner) {
                            Debug.LogError ("There needs to be one active EventManger script on a GameObject in your scene.");
                        } else {
                            _eventListner.Init ();
                        }
                    }
                }

                return _eventListner;
            }
        }

        private void Init () {
            if (eventDictionary == null) {
                eventDictionary = new Dictionary<MessageEventSignature, UnityEvent> ();
            }
        }

        protected static void StartListening (MessageEventSignature eventSignature, UnityAction listener) {
			if (Instance.eventDictionary.TryGetValue (eventSignature, out UnityEvent thisEvent)) {
				thisEvent.AddListener (listener);
			} else {
				thisEvent = new UnityEvent ();
				thisEvent.AddListener (listener);
				Instance.eventDictionary.Add (eventSignature, thisEvent);
			}
		}

        public static void StopListening (MessageEventSignature eventSignature) {
            if (_eventListner == null)
                return;

			if (Instance.eventDictionary.TryGetValue (eventSignature, out UnityEvent thisEvent)) {
				thisEvent.RemoveAllListeners ();
			}
		}

        public static void TriggerEvent (MessageEventSignature eventSignature) {
			if (Instance.eventDictionary.TryGetValue (eventSignature, out UnityEvent thisEvent)) {
				thisEvent.Invoke ();
			}
		}
    }
}
