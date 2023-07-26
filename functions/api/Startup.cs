using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using VideoIndexerConsole.Functions.Common;

[assembly: FunctionsStartup(typeof(VideoIndexerConsole.Functions.Api.Startup))]

namespace VideoIndexerConsole.Functions.Api
{
    class Startup : FunctionsStartup
    {
        public IConfiguration Configuration { get; }

        public Startup()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", true);
            Configuration = config.Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new FunctionConfiguration(Configuration);
            var credential = new DefaultAzureCredential();
            var blobServiceClient = new BlobServiceClient(
                new Uri($"https://{configuration.StorageAccountName}.blob.core.windows.net"), 
                credential);

            // BlobServiceClient
            builder.Services.AddSingleton(provider => blobServiceClient);

            // BlobSasGenerator
            builder.Services.AddSingleton(provider =>
            {
                var blobSasGenerator = new BlobSasGenerator(blobServiceClient, configuration.StorageContainerName);
                return blobSasGenerator;
            });

            // VideoIndexerClient
            builder.Services.AddSingleton(provider =>
            {
                var videoIndexerSettings = new VideoIndexerClientSettings
                {
                    Credential = credential,
                    VideoIndexerAccountId = configuration.VideoIndexerAccountId,
                    VideoIndexerLocation = configuration.VideoIndexerLocation,
                    VideoIndexerResourceId = configuration.VideoIndexerResourceId
                };
                var videoIndexerClient = new VideoIndexerClient(videoIndexerSettings);
                return videoIndexerClient;
            });

            // FunctionConfiguration
            builder.Services.AddSingleton(provider => configuration);
        }
    }
}
