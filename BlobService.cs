using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using file_fetching_api.Repositories;

namespace file_fetching_api.Services { 

    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName; 
        private readonly ILogger<BlobService> _logger; 


        public BlobService(IConfiguration configuration, ILogger<BlobService> logger, IFilesRepository filesRepository)
        {
            var connectionString = configuration.GetValue<string>("AzureBlobStorage:ConnectionString"); // accessing the appsettings.json 
            _containerName = configuration.GetValue<string>("AzureBlobStorage:ContainerName");
            _blobServiceClient = new BlobServiceClient(connectionString);
            _logger = logger;
        }


        // DOWNLOADING A FILE 
        public async Task<Stream> GetFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            try
            {
                _logger.LogInformation("Checking if blob exists: {BlobName}", fileName);
                if (await blobClient.ExistsAsync())
                {
                    _logger.LogInformation("Blob found, downloading: {BlobName}", fileName);
                    var blobDownloadInfo = await blobClient.DownloadAsync();
                    return blobDownloadInfo.Value.Content;
                }
                else
                {
                    _logger.LogWarning("Blob not found: {BlobName}", fileName);
                    throw new FileNotFoundException($"File '{fileName}' not found in Blob Storage.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the file: {BlobName}", fileName);
                throw new InvalidOperationException("An error occurred while retrieving the file.", ex);
            }
        }


        // UPLOADING A FILE


    }
}