using Azure.Storage;
using Azure.Storage.Blobs;
using KeyVaultDemo.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KeyVaultDemo.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly StorageConfig storageConfig;

        public FileUploadController(StorageConfig storageConfig)
        {
            this.storageConfig = storageConfig;
        }

        [HttpPost("FileUpload")]
        public async Task<IActionResult> Index(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            string fileNamePattern = @"^[\w\-. ]+$";

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    bool shouldProcess = Regex.IsMatch(formFile.FileName, fileNamePattern);
                    if (shouldProcess)
                    {
                        filePaths.Add(formFile.FileName);
                        await UploadFileToStorage(formFile.OpenReadStream(), formFile.FileName);
                    }
                }
            }

            return Ok(new { message = "Files uploaded to Blob", count = files.Count, size, filePaths });
        }


        private async Task<bool> UploadFileToStorage(Stream fileStream, string fileName)
        {
            //// STEP 1: Create Blob URI
            string blobHost = $"https://{storageConfig.AccountName}.blob.core.windows.net/";
            string containerFilePart = $"{storageConfig.ContainerName}/{fileName}";
            Uri blobUri = new Uri($"{blobHost}{containerFilePart}");

            // STEP 2: Storage Credentials
            StorageSharedKeyCredential storageCredentials
                = new StorageSharedKeyCredential(storageConfig.AccountName,
                                                    storageConfig.Key);

            // STEP 3: Upload using BlobClient object
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);
            await blobClient.UploadAsync(fileStream);

            return await Task.FromResult(true);
        }

    }
}
