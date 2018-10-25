using System;
using Microsoft.AspNetCore.Http;

namespace CalMan.Entities
{
    public class SlackPostEntity
    {
        public string Token { get; }
        public string TeamId { get; }
        public string TeamDomain { get; }
        public string ChannelId { get; }
        public string ChannelName { get; }
        public TimeSpan Timestamp { get; }
        public string UserId { get; }
        public string UserName { get; }
        public string Text { get; }
        public string TriggerWord { get; }

        public SlackPostEntity(IFormCollection requestForm)
        {
            Token = requestForm.TryGetValue("token", out var token) ? token : throw new ArgumentException($"\"token\" is not found. {requestForm}");
            TeamId = requestForm.TryGetValue("team_id", out var team_id) ? team_id : throw new ArgumentException($"\"team_id\" is not found. {requestForm}");
            TeamDomain = requestForm.TryGetValue("team_domain", out var team_domain) ? team_domain : throw new ArgumentException($"\"team_domain\" is not found. {requestForm}");
            ChannelId = requestForm.TryGetValue("channel_id", out var channel_id) ? channel_id : throw new ArgumentException($"\"channel_id\" is not found. {requestForm}");
            ChannelName = requestForm.TryGetValue("channel_name", out var channel_name) ? channel_name : throw new ArgumentException($"\"channel_name\" is not found. {requestForm}");
            // Todo: mapping unixtime + micro sec
            // Timestamp = requestForm.TryGetValue("timestamp", out var timestamp) ?  timestamp : throw new ArgumentException($"\"token\" is not found. {requestForm}");
            UserId = requestForm.TryGetValue("user_id", out var user_id) ? user_id : throw new ArgumentException($"\"user_id\" is not found. {requestForm}");
            UserName = requestForm.TryGetValue("user_name", out var user_name) ? user_name : throw new ArgumentException($"\"user_name\" is not found. {requestForm}");
            Text = requestForm.TryGetValue("text", out var text) ? text : throw new ArgumentException($"\"text\" is not found. {requestForm}");
            TriggerWord = requestForm.TryGetValue("trigger_word", out var trigger_word) ? trigger_word : throw new ArgumentException($"\"trigger_word\" is not found. {requestForm}");
        }
    }
}