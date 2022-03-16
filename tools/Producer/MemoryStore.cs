using System.Collections.Concurrent;
using Shared.Messages;

namespace Producer
{
    public static class MemoryStore
    {
        private static readonly ConcurrentDictionary<string, Message> _messages = new ConcurrentDictionary<string, Message>();
        private static readonly ConcurrentDictionary<string, ReplyMessage> _replies = new ConcurrentDictionary<string, ReplyMessage>();

        public static IEnumerable<Message> GetMessages() => _messages.Values.ToArray();
        public static IEnumerable<ReplyMessage> GetReplies() => _replies.Values.ToArray();
        public static void Save(params Message[] messages)
        {
            foreach (var message in messages)
                _messages.TryAdd(message.Id, message);
        }

        public static void Save(ReplyMessage message)
        {
            ReplyMessage old = null;

            if (_replies.ContainsKey(message.MessageId))
                _replies.Remove(message.MessageId, out old);

            if (old != null)
                message.Attempts = old.Attempts + 1;
            else
                message.Attempts++;

            _replies.TryAdd(message.MessageId, message);
        }
    }
}
