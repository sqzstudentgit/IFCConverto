using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;

namespace IFCConverto.Services
{
    class S3UploadService
    {         
        private string bucketName;
        private string awsAccessKey;
        private string awsSecretKey;
        private const string awsSubFolder = "3dModels";

        public S3UploadService(string bucket, string accesskey, string secretkey)
        {
            bucketName = bucket;
            awsAccessKey = accesskey;
            awsSecretKey = secretkey;            
        }

        /// <summary>
        /// This method is used for uploading the files to the S3 bucket.
        /// </summary>
        /// <param name="filePath">path of the file</param>
        /// <param name="filename">name of the file</param>
        /// <returns>Url of the uploaded object</returns>
        public string UploadFile(string filePath, string filename)
        {

            IAmazonS3 client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.APSoutheast2);

            FileInfo file = new FileInfo(filePath);
            string path = awsSubFolder+"/" + filename;

            PutObjectRequest request = new PutObjectRequest()
            {
                InputStream = file.OpenRead(),
                BucketName = bucketName,
                Key = path 
            };

            PutObjectResponse response = client.PutObject(request);

            // Changing the access permissions to public
            client.PutACL(new PutACLRequest
            {
                BucketName = bucketName,
                Key = path,
                CannedACL = S3CannedACL.PublicRead
            });

            // Generating the URL to access the 3D Model.
            string endpointURL = "ap-southeast-2";
            string url = "https://s3-" + endpointURL + ".amazonaws.com/" + bucketName + "/"+awsSubFolder+"/" + filename;
            
            return url;
        }

        /// <summary>
        /// This method will create the folder on the S3 bucket to store the images. 
        /// </summary>
        public void CreateFolder()
        {
            var client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.APSoutheast2);
            string folderPath = awsSubFolder + "/";

            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = folderPath
            };

            client.PutObject(request);
        }

    }
}
