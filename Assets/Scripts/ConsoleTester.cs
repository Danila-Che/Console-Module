using UnityEngine;
using System.Collections.Generic;
using ConsoleModule;

public class ConsoleTester: MonoBehaviour {
	private List<MessageEventSignature> _messageEnvets = new List<MessageEventSignature> ();

	private void OnEnable () {
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Заслонка закрыта"));
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Заслонка открыта"));
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Эта запись не логируется", log: false));
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddWarning ("Проверьте положение заслонки"));
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddError ("Произошла остановка имитатора впускного коллектора"));

		foreach (MessageEventSignature message in _messageEnvets) {
			ConsoleMessageHandler.TriggerEvent (message);
			Debug.Log (message.Name);
		}
	}

	private void OnDisable () {
		ConsoleMessageHandler.StopListening (_messageEnvets [0]);
		ConsoleMessageHandler.StopListening (_messageEnvets [1]);
		ConsoleMessageHandler.StopListening (_messageEnvets [2]);
		ConsoleMessageHandler.StopListening (_messageEnvets [3]);
	}

	private void Update () {
		if (Input.GetKey (KeyCode.LeftControl) && Input.GetKeyDown (KeyCode.Space)) {
			ConsoleMessageHandler.TriggerEvent (_messageEnvets [Random.Range (0, _messageEnvets.Count)]);
		}
	}
}
