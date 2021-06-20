using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Colorful.Web.Models.Webhook
{
    /// <summary>
    /// Utility class for creating discord webhook messages.
    /// </summary>
    public class Webhook
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;

        public Webhook(ulong id, string token) : this($"https://discordapp.com/api/webhooks/{id}/{token}") { }

        public Webhook(string webhookUrl)
        {
            _httpClient = new HttpClient();
            _webhookUrl = webhookUrl;
        }

        public Task<HttpResponseMessage> Send(string content)
        {
            return Send(new WebhookMessage() { Content = content });
        }

        public async Task<HttpResponseMessage> Send(WebhookMessage msg)
        {
            var content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json");

            var resp = await _httpClient.PostAsync(_webhookUrl, content);
            await Task.Delay(200); //Just to try and make discord do it properly
            return resp;
        }

    }
}
