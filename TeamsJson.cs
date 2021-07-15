using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

namespace SonarqAzureDevopsIntegration
{
    public class TeamsJson
    {
        [JsonProperty("value")]
        public Value[] Teams { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }
    }

    public partial class Value
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("identityUrl")]
        public Uri IdentityUrl { get; set; }

        [JsonProperty("projectName")]
        public ProjectName ProjectName { get; set; }

        [JsonProperty("projectId")]
        public Guid ProjectId { get; set; }
    }

    public enum ProjectName { Projects, Yna };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ProjectNameConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ProjectNameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ProjectName) || t == typeof(ProjectName?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Projects":
                    return ProjectName.Projects;
                case "YNA":
                    return ProjectName.Yna;
            }
            throw new Exception("Cannot unmarshal type ProjectName");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (ProjectName)untypedValue;
            switch (value)
            {
                case ProjectName.Projects:
                    serializer.Serialize(writer, "Projects");
                    return;
                case ProjectName.Yna:
                    serializer.Serialize(writer, "YNA");
                    return;
            }
            throw new Exception("Cannot marshal type ProjectName");
        }

        public static readonly ProjectNameConverter Singleton = new ProjectNameConverter();
    }
}