using System.Collections.Generic;

namespace ConsoleModule {
	public interface ISavedMessage {
		public List<Message> SavedMessages { get; }
	}
}
