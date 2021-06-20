using Newtonsoft.Json;

namespace Colorful.Web.Models.Webhook
{
    public class WebhookMessage
    {
        [JsonProperty("content")]
        private string content;
        public string Content
        {
            get => content;
            set => content = ValidateContentLength(value);
        }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("tts")]
        public bool IsTTS { get; set; }

        [JsonProperty("file")]
        public object File { get; set; }

        private string ValidateContentLength(string message)
        {
            return message.Length > 2000 ? message.Substring(0, 2000) : message;
        }
    }
}