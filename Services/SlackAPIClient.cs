using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CalMan.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CalMan.Services
{
    public class SlackAPIClient
    {
        private static SlackAPIClient instance = new SlackAPIClient();
        private HttpClient client;
        public string SlackAPIKey { private get; set; }
        public static SlackAPIClient Instance
        {
            get
            {
                return instance;
            }
        }
        private SlackAPIClient()
        {
            client = new HttpClient();
        }

        public async Task<IEnumerable<string>> GetGroupUsers(string groupId)
        {
            var res = await client.GetAsync($"https://slack.com/api/usergroups.users.list?token={SlackAPIKey}&usergroup={groupId}");
            var json = JObject.Parse(await res.Content.ReadAsStringAsync());
            if (json.TryGetValue("ok", out var ok) && ok.Value<bool>())
            {
                return json.GetValue("users").Values<string>();
            }
            return new List<string>();
        }

        public async Task<IEnumerable<SlackUserEntity>> GetUsers()
        {
            var members = new List<SlackUserEntity>();
            string cursor = null;
            while (true)
            {
                var reqUrl = $"https://slack.com/api/users.list?token={SlackAPIKey}" + (cursor != null ? $"&cursor={cursor}" : "");
                var res = await client.GetAsync(reqUrl);
                var json = JsonConvert.DeserializeObject<SlackUserEntiryWrapper>(await res.Content.ReadAsStringAsync());

                if (!json.Ok) break;
                members.AddRange(json.Members);

                if (json.ResponseMetadata == null || json.ResponseMetadata.NextCursor == "") break;
                cursor = json.ResponseMetadata.NextCursor;
            }

            return members;
        }
    }
}