using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Threading.Tasks;
using VideoIndexerConsole.Functions.Common;

namespace VideoIndexerConsole.Functions.Api
{
    /// <summary>
    /// ビデオファイルを Azure Storage Blob にアップロードするための URL を発行する Web API
    /// </summary>
    class GetVideoUploadUrl
    {
        readonly BlobSasGenerator _sasGenerator;

        public GetVideoUploadUrl(BlobSasGenerator sasGenerator)
        {
            _sasGenerator = sasGenerator;
        }

        [FunctionName(nameof(GetVideoUploadUrl))]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "videos/uploadurl")] HttpRequest req)
        {
            // クエリストリングからBlobへアップロードするファイル名を取得する
            const string queryKey = "name";
            if (!req.Query.ContainsKey(queryKey)) return new BadRequestResult();
            var blobName = req.Query[queryKey];

            // ビデオファイルをアップロードするための SAS を発行して、アップロード用のURLを生成する
            var urlWithSas = await _sasGenerator.GetUrlWithSasAsync(blobName, BlobSasPermissions.Write, DateTime.UtcNow.AddMinutes(3));
            return new OkObjectResult(urlWithSas);
        }
    }
}
