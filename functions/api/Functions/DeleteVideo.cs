using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using VideoIndexerConsole.Functions.Common;

namespace VideoIndexerConsole.Functions.Api
{
    /// <summary>
    /// Azure Video Indexer からビデオのを削除するための Web API
    /// </summary>
    class DeleteVideo
    {
        readonly VideoIndexerClient _indexerClient;

        public DeleteVideo(VideoIndexerClient indexerClient)
        {
            _indexerClient = indexerClient;
        }

        [FunctionName(nameof(DeleteVideo))]
        public async Task RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "videos/{videoId}")] HttpRequest req,
            string videoId)
        {
            await _indexerClient.DeleteVideoAsync(videoId);
        }
    }
}
