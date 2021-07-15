using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Globalization;

namespace Sonar2PBI
{
    public partial class IterationJson
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public Iteration[] Iterations { get; set; }
    }

    public partial class Iteration
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class Attributes
    {
        [JsonProperty("startDate")]
        public DateTimeOffset? StartDate { get; set; }

        [JsonProperty("finishDate")]
        public DateTimeOffset? FinishDate { get; set; }

        [JsonProperty("timeFrame")]
        public TimeFrame TimeFrame { get; set; }
    }

    public enum TimeFrame { Current, Future, Past };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                TimeFrameConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class TimeFrameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TimeFrame) || t == typeof(TimeFrame?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "current":
                    return TimeFrame.Current;
                case "future":
                    return TimeFrame.Future;
                case "past":
                    return TimeFrame.Past;
            }
            throw new InvalidOperationException("Cannot unmarshal type TimeFrame");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TimeFrame)untypedValue;
            switch (value)
            {
                case TimeFrame.Current:
                    serializer.Serialize(writer, "current");
                    return;
                case TimeFrame.Future:
                    serializer.Serialize(writer, "future");
                    return;
                case TimeFrame.Past:
                    serializer.Serialize(writer, "past");
                    return;
            }
            throw new InvalidOperationException("Cannot marshal type TimeFrame");
        }

        public static readonly TimeFrameConverter Singleton = new TimeFrameConverter();
    }
}