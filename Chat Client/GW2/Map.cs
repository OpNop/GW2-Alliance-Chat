using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Chat_Client
{
    public partial class Map
    {
        [JsonProperty("map_name")]
        public string MapName { get; set; }

        [JsonProperty("min_level")]
        public long MinLevel { get; set; }

        [JsonProperty("max_level")]
        public long MaxLevel { get; set; }

        [JsonProperty("default_floor")]
        public long DefaultFloor { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("floors")]
        public List<long> Floors { get; set; }

        [JsonProperty("region_id")]
        public long RegionId { get; set; }

        [JsonProperty("region_name")]
        public string RegionName { get; set; }

        [JsonProperty("continent_id")]
        public long ContinentId { get; set; }

        [JsonProperty("continent_name")]
        public string ContinentName { get; set; }

        [JsonProperty("map_rect")]
        public List<List<long>> MapRect { get; set; }

        [JsonProperty("continent_rect")]
        public List<List<long>> ContinentRect { get; set; }
    }

    public partial class Map
    {
        public static Dictionary<int, Map> Load()
        {
            //Load Map Data JSON
            string MapJson;
            using (StreamReader r = new StreamReader("GW2/MapData.json"))
            {
                MapJson = r.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<Dictionary<int, Map>>(MapJson, Converter.Settings);
        }
    }

    public static class Serialize
    {
        public static string ToJson(this Dictionary<string, Map> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
