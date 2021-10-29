using UnityEngine;
using System.Collections.Generic;
using ConsoleModule;

public class ConsoleTester: MonoBehaviour {
	private List<MessageEventSignature> _messageEnvets = new List<MessageEventSignature> ();

	private void OnEnable () {
<<<<<<< HEAD
		_messageEnvets.Add (ConsoleMessageHandler.AddNotification ("Заслонка закрыта"));
		_messageEnvets.Add (ConsoleMessageHandler.AddNotification ("Заслонка открыта"));
		_messageEnvets.Add (ConsoleMessageHandler.AddNotification ("Эта запись не логируется", log: false));
		_messageEnvets.Add (ConsoleMessageHandler.AddWarning ("Проверьте положение заслонки"));
		_messageEnvets.Add (ConsoleMessageHandler.AddError ("Произошла остановка имитатора впускного коллектора"));
=======
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Р—Р°СЃР»РѕРЅРєР° Р·Р°РєСЂС‹С‚Р°"));
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Р—Р°СЃР»РѕРЅРєР° РѕС‚РєСЂС‹С‚Р°"));
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Р­С‚Р° Р·Р°РїРёСЃСЊ РЅРµ Р»РѕРіРёСЂСѓРµС‚СЃСЏ", log: false));
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddWarning ("РџСЂРѕРІРµСЂСЊС‚Рµ РїРѕР»РѕР¶РµРЅРёРµ Р·Р°СЃР»РѕРЅРєРё"));
		_messageEnvets.Add (ConsoleMessageHandler.Instance.AddError ("РџСЂРѕРёР·РѕС€Р»Р° РѕСЃС‚Р°РЅРѕРІРєР° РёРјРёС‚Р°С‚РѕСЂР° РІРїСѓСЃРєРЅРѕРіРѕ РєРѕР»Р»РµРєС‚РѕСЂР°"));
>>>>>>> 521fa8ec97273c1cf2342a7a78a6189712c66555

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
