using Azure.Messaging.EventGrid;
using Azure.Storage.Sas;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using VideoIndexerConsole.Functions.Common;

namespace VideoIndexerConsole.Functions.Backend
{
    public class ProcessUploadedData
    {
        readonly VideoIndexerClient _videoIndexerClient;
        readonly BlobSasGenerator _sasGenerator;

        public ProcessUploadedData(VideoIndexerClient videoIndexerClient, BlobSasGenerator sasGenerator)
        {
            _videoIndexerClient = videoIndexerClient;
            _sasGenerator = sasGenerator;
        }

        [FunctionName(nameof(ProcessUploadedData))]
        public async Task RunAsync([EventGridTrigger] EventGridEvent e, ILogger logger)
        {
            // 入力を取得する
            var data = e.Data.ToString();
            var blobEvent = JsonConvert.DeserializeObject<BlobEvent>(data);
            logger.LogInformation($"Inputed Data: {data}");

            // アップロードされた Blob 情報を取得する
            var url = new Uri(blobEvent.Url);
            var storageAccountName = url.Host.Replace(".blob.core.windows.net", string.Empty);
            var containerName = url.LocalPath.Split("/")[1];
            var blobName = url.LocalPath.Replace($"/{containerName}/", "");

            // Video Indexer が使用するための Blob の SAS + URL を生成する
            var urlWithSas = await _sasGenerator.GetUrlWithSasAsync(blobName, BlobSasPermissions.Read, DateTime.UtcNow.AddMinutes(5));

            // Video Indexer に分析処理リクエストを送る
            await _videoIndexerClient.UploadVideoAsync(urlWithSas, blobName);
        }
    }
}