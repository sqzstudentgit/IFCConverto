using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace IFCConverto.Services
{
    class S3UploadService
    {
         
        string _bucketName;
        string _awsAccessKey;
        string _awsSecretKey;

        string awsSubFolder;

        public S3UploadService(string bucket, string accesskey, string secretkey)
        {
            _bucketName = bucket;
            _awsAccessKey = accesskey;
            _awsSecretKey = secretkey;

            awsSubFolder = "3dModels";
        }

        public string UploadFile(string filePath, string filename)
        {

            IAmazonS3 client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, RegionEndpoint.APSoutheast2);

            FileInfo file = new FileInfo(filePath);
            string path = awsSubFolder+"/" + filename;

            PutObjectRequest request = new PutObjectRequest()
            {
                InputStream = file.OpenRead(),
                BucketName = _bucketName,
                Key = path 
            };

            PutObjectResponse response = client.PutObject(request);

            // Changing the access permissions to public
            client.PutACL(new PutACLRequest
            {
                BucketName = _bucketName,
                Key = path,
                CannedACL = S3CannedACL.PublicRead
            });

            // Generating the URL to access the 3D Model.
            string endpointURL = "ap-southeast-2";
            string url = "https://s3-" + endpointURL + ".amazonaws.com/" + _bucketName + "/"+awsSubFolder+"/" + filename;
            
            return url;
        }
        public void CreateFolder()
        {
            IAmazonS3 client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, RegionEndpoint.APSoutheast2);
            string folderPath = awsSubFolder + "/";

            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = folderPath
            };

            PutObjectResponse response = client.PutObject(request);
        }

    }
}
