using System.Collections.Concurrent;
using Application.DTOs.AI;

namespace Application.Services
{
    public class AiConversationMemoryService
    {
        private const int MaxMessagesPerConversation = 24;
        private static readonly TimeSpan ConversationTtl = TimeSpan.FromHours(6);

        private readonly ConcurrentDictionary<string, ConversationState> _conversations = new();

        public List<AiChatMessageDto> GetHistory(string conversationKey)
        {
            if (!_conversations.TryGetValue(conversationKey, out var state))
            {
                return new List<AiChatMessageDto>();
            }

            if (DateTime.UtcNow - state.LastUpdatedUtc > ConversationTtl)
            {
                _conversations.TryRemove(conversationKey, out _);
                return new List<AiChatMessageDto>();
            }

            lock (state.Lock)
            {
                state.LastUpdatedUtc = DateTime.UtcNow;
                return state.Messages.Select(message => new AiChatMessageDto
                {
                    Role = message.Role,
                    Content = message.Content
                }).ToList();
            }
        }

        public void SaveHistory(string conversationKey, IReadOnlyList<AiChatMessageDto> messages)
        {
            var state = _conversations.GetOrAdd(conversationKey, _ => new ConversationState());

            lock (state.Lock)
            {
                state.Messages = messages
                    .TakeLast(MaxMessagesPerConversation)
                    .Select(message => new AiChatMessageDto
                    {
                        Role = message.Role,
                        Content = message.Content
                    })
                    .ToList();
                state.LastUpdatedUtc = DateTime.UtcNow;
            }
        }

        private sealed class ConversationState
        {
            public object Lock { get; } = new();
            public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
            public List<AiChatMessageDto> Messages { get; set; } = new();
        }
    }
}
