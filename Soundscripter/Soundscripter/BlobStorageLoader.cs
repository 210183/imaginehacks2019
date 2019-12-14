using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace Soundscripter
{
    public class BlobStorageLoader
    {
        public string BlobName { get; set; } = "samples-wdzz5oxrxcpoddp3bqa3b".ToLowerInvariant();

        public BlobServiceClient BlobServiceClient { get; } =
            new BlobServiceClient(Environment.GetEnvironmentVariable("BLOB_CONNECT_STR"));

        public async Task<string> PutIntoBlob(string audioPath)
        {
            BlobContainerClient containerClient = BlobServiceClient.GetBlobContainerClient(BlobName);
            string blobFileName = Path.ChangeExtension(Guid.NewGuid().ToString().Substring(0, 12).ToLowerInvariant(), ".mp3");
            BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            await using FileStream uploadFileStream = File.OpenRead(audioPath);
            await blobClient.UploadAsync(uploadFileStream);
            uploadFileStream.Close();
            return blobFileName;
        }
        //public async void GetAllBlobs()
        //{
        //    BlobContainerClient containerClient = BlobServiceClient.GetBlobContainerClient(BlobName);
        //    List<MemoryStream> downloadedBlobs
        //    await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        //    {
        //        // Download the blob's contents and save it to a file
        //        BlobDownloadInfo download = await containerClient.GetBlobClient(blobItem.Name).DownloadAsync();
        //        download.Content
        //        using FileStream downloadFileStream = File.OpenWrite();
        //        await download.Content.CopyToAsync(downloadFileStream);
        //        downloadFileStream.Close();
        //    }
        //}
    }
}
