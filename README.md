Тестовое задание по созданию модуля консоли вывода сообщений тестирования.
[Документ задания!](https://github.com/Danila-Che/Console-Module/blob/main/%D0%97%D0%B0%D0%B4%D0%B0%D0%BD%D0%B8%D0%B5%20-%20(%D0%9C%D0%BE%D0%B4%D1%83%D0%BB%D1%8C%20Unity).docx)

Модуль доступен из любой точки в коде для добавления сообщений.
Сам же модуль расположен на отдельный сцене.
Но если так не нравиться, то можно добавить DontDestroyOnLoad код [link to Unity!](https://docs.unity3d.com/ScriptReference/Object.DontDestroyOnLoad.html).

Весь базовый функционал можно посмотреть в коде класса ConsoleTester
Функционал: после добавления сообщения вощращается подпись на событие сообщения.
Это сделанно для удобства использования и строгости (без неё можно переделать код, используя сроковый тип. Но это бы внесло запутанности в код пользователя).
Тем не меннее MessageEventSignature содержит поле для имени
```C#
private List<MessageEventSignature> _messageEnvets = new List<MessageEventSignature> ();

private void OnEnable () {
	_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Заслонка закрыта"));
	_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Заслонка открыта"));
	_messageEnvets.Add (ConsoleMessageHandler.Instance.AddNotification ("Эта запись не логируется", log: false));
	_messageEnvets.Add (ConsoleMessageHandler.Instance.AddWarning ("Проверьте положение заслонки"));
	_messageEnvets.Add (ConsoleMessageHandler.Instance.AddError ("Произошла остановка имитатора впускного коллектора"));
}

private void OnDisable () {
	ConsoleMessageHandler.StopListening (_messageEnvets [0]);
	ConsoleMessageHandler.StopListening (_messageEnvets [1]);
	ConsoleMessageHandler.StopListening (_messageEnvets [2]);
	ConsoleMessageHandler.StopListening (_messageEnvets [3]);
	ConsoleMessageHandler.StopListening (_messageEnvets [4]);
}

private void Update () {
	if (Input.GetKey (KeyCode.LeftControl) && Input.GetKeyDown (KeyCode.Space)) {
		ConsoleMessageHandler.TriggerEvent (_messageEnvets [Random.Range (0, _messageEnvets.Count)]);
	}
}
```

Обработчик сообщений является Event Listener'ом, и в его основе лежит синглетон. Поэтому можно вызвать его из любой сцены.
У подписи на события (MessageEventSignature) есть поле имени для удобства обращения и опознавания события
