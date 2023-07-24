using Newtonsoft.Json;

namespace VideoIndexerConsole.Functions.Backend
{
    class BlobEvent
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}