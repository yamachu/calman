using System.Collections.Generic;
using Newtonsoft.Json;

namespace CalMan.Entities
{
    public class SlackUserEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ProfileEntity Profile { get; set; }
        public class ProfileEntity
        {
            public string Email { get; set; }
        }
    }

    // 必要なパラメータだけに絞った
    // ref: https://api.slack.com/methods/users.list
    public class SlackUserEntiryWrapper
    {
        public bool Ok { get; set; }
        public IEnumerable<SlackUserEntity> Members { get; set; }

        [JsonProperty("response_metadata")]
        public MetaData ResponseMetadata { get; set; }
        public class MetaData
        {
            [JsonProperty("next_cursor")]
            public string NextCursor { get; set; }
        }
    }

}