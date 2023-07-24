using Azure.Core;

namespace VideoIndexerConsole.Functions.Common
{
    public class VideoIndexerClientSettings
    {
#nullable disable warnings
        public TokenCredential Credential { get; set; }
        public string VideoIndexerLocation { get; set; }
        public string VideoIndexerAccountId { get; set; }
        public string VideoIndexerResourceId { get; set; }
    }
}