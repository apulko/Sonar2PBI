using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sonar2PBI
{
    public class TeamMemberJson
    {

        [JsonProperty("value")]
        public Team[] TeamMembers { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }
    }

    public partial class Team
    {
        [JsonProperty("identity")]
        public Identity Identity { get; set; }

        [JsonProperty("isTeamAdmin", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTeamAdmin { get; set; }
    }

    public partial class Identity
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }
    }

    public partial class Links
    {
        [JsonProperty("avatar")]
        public Avatar Avatar { get; set; }
    }

    public partial class Avatar
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }
}