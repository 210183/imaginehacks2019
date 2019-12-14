using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

namespace Soundscripter
{
    public class BucketLoader
    {
        private string ProjectName { get; } = "cogent-elevator-261920";
        private string BucketName { get; }
        public StorageClient StorageClient { get; } = StorageClient.Create();

        public BucketLoader(string bucketName = "temp-bucket")
        {
            BucketName = (bucketName + Guid.NewGuid().ToString().Substring(0, 20) + 'a').ToLowerInvariant();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="objectName"></param>
        /// <returns>URI for created file</returns>
        public (string fullUri, string objectName) UploadFileFromLocal(string localPath, string objectName = null)
        {
            Bucket bucket = StorageClient.CreateBucket(ProjectName, BucketName);
            using Stream f = File.OpenRead(localPath);
            objectName = objectName ?? Path.GetFileName(localPath);
            StorageClient.UploadObject(BucketName, objectName, null, f);
            Console.WriteLine($"Uploaded {objectName}.");
            return ($"gs://{bucket.Name}/{objectName}", objectName);
        }

        private Bucket CreateBucket(StorageClient client)
        {
            Bucket bucket = client.CreateBucket(BucketName, BucketName);
            Console.WriteLine($"Created {BucketName}.");
            return bucket;
        }

        public void DeleteObject(IEnumerable<string> objectNames)
        {
            foreach (string objectName in objectNames)
            {
                StorageClient.DeleteObject(BucketName, objectName);
                Console.WriteLine($"Deleted {objectName}.");
            }
        }
    }
}
