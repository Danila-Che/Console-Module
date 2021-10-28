using System;
using UnityEngine;

namespace ConsoleModule {
	public class Message {
		public Message (string text, DateTime time, Color color, LogsTypes logsType) {
			Text = text;
			Time = time;
			Color = color;
			LogsType = logsType;
		}

		public string Text { get; }
		public DateTime Time { get; }
		public Color Color { get; }
		public LogsTypes LogsType { get; }
	}
}
