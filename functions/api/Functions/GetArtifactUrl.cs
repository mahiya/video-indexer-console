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
    class GetArtifactUrl
    {
        readonly VideoIndexerClient _indexerClient;

        public GetArtifactUrl(VideoIndexerClient indexerClient)
        {
            _indexerClient = indexerClient;
        }

        [FunctionName(nameof(GetArtifactUrl))]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "videos/{videoId}/artifact")] HttpRequest req,
            string videoId)
        {
            var artifactUrl = await _indexerClient.GetVideoArtifactDownloadUrlAsync(videoId);
            return new OkObjectResult(new { artifactUrl });
        }
    }
}
