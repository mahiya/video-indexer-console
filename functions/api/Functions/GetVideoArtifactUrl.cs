using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using VideoIndexerConsole.Functions.Common;

namespace VideoIndexerConsole.Functions.Api
{
    /// <summary>
    /// ビデオの分析結果ファイルをダウンロードするためのURLを返す Web API
    /// </summary>
    class GetVideoArtifactUrl
    {
        readonly VideoIndexerClient _indexerClient;

        public GetVideoArtifactUrl(VideoIndexerClient indexerClient)
        {
            _indexerClient = indexerClient;
        }

        [FunctionName(nameof(GetVideoArtifactUrl))]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "videos/{videoId}/{artifactType}/url")] HttpRequest req,
            string videoId,
            string artifactType)
        {
            var url = await _indexerClient.GetVideoArtifactDownloadUrlAsync(videoId, artifactType);
            return new OkObjectResult(new { url });
        }
    }
}
