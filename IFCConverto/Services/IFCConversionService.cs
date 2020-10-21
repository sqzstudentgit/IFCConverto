using IFCConverto.Models;
using IFCConvertoLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Windows.Documents;
using static IFCConverto.Enums.DestinationLocationTypeEnum;
using static IFCConverto.Enums.IFCConvertEnum;

namespace IFCConverto.Services
{
    public class IFCConversionService
    {
        public delegate void ConversionUpdateForUIEventHandler(string message);
        public event ConversionUpdateForUIEventHandler TotalFiles;
        public event ConversionUpdateForUIEventHandler RemainingFiles;
        public event ConversionUpdateForUIEventHandler RemainingModels;
        public event ConversionUpdateForUIEventHandler ConversionException;

        /// <summary>
        /// Method to start converting the ifc files in to the GLTF format
        /// </summary>
        /// <param name="sourceLocation">Source location path</param>
        /// <param name="destinationLocation">Destination location path</param>
        /// <returns>IFC convert Status to update the UI</returns>
        public async Task<IFCConvertStatus> ConvertFiles(string sourceLocation, string destinationLocation, 
            string bucket, string accesskey, string secretkey, DestinationLocationType destinationType)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // get all file names from the source location
                    var allFilenames = Directory.EnumerateFiles(sourceLocation).Select(p => Path.GetFileName(p));

                    // Get all filenames that have a .ifc extension
                    var files = allFilenames.Where(fn => Path.GetExtension(fn) == ".ifc");

                    // If there are no files, then return to notify the user
                    if (files == null || files.Count() == 0)
                    {
                        return IFCConvertStatus.NoFiles;
                    }

                    // Get total number of files, (need it for the progress bar)
                    var totalFiles = files.Count();

                    // Send the total count of the files to the viewmodel for update on UI
                    TotalFiles?.Invoke(totalFiles.ToString());
                    
                    // Process each file and convert it 
                    foreach (var file in files)
                    {                        
                        var sourceFile = Path.Combine(sourceLocation, file);
                        var filePathWithGLTFExtentsion = Path.ChangeExtension(file, ".glb");
                        var destinationFile = Path.Combine(destinationLocation, filePathWithGLTFExtentsion);
                        IFCConvert.Convert(sourceFile, destinationFile);
                        totalFiles--;

                        // Send message to UI to update progress bar
                        RemainingFiles?.Invoke((totalFiles).ToString());
                    }

                    // Send a call to the S3 bucket to upload data and the API
                    if (destinationType == DestinationLocationType.Both)
                    {
                        var productList = UploadModels(files, bucket, accesskey, secretkey, sourceLocation, destinationLocation);
                        _ = SendDataToAPI(productList);                        
                    }                                       

                    // Return success message
                    return IFCConvertStatus.Done;
                });                
            }
            catch (Exception ex)
            {
                ConversionException?.Invoke(ex.Message);
                return IFCConvertStatus.Error;
            }
        }

        private async Task<bool> SendDataToAPI(List<Products> productList)
        {
            var serverDetails = new SettingsService().LoadSettings();

            var productData = new ProductData
            {
                Username = serverDetails.Username,
                Password = serverDetails.Password,
                Products = productList
            };

            var data = JsonConvert.SerializeObject(productData);

            return await HttpService.PostModelLinks(productData, serverDetails.ServerURL);
        }

        private List<Products> UploadModels(IEnumerable<string> files, string bucket, string accesskey, string secretkey, string sourceLocation, string destinationLocation)
        {
            // Initialization                        
            var uploader = new S3UploadService(bucket, accesskey, secretkey);
            var tasks = new List<Task>();
            var sourceFiles = new List<string>();
            var productUrls = new List<Products>();

            // Create 3dModel Folder on AWS to upload the models
            uploader.CreateFolder();

            // Get the List of all 3D Models
            foreach (var file in files)
            {
                var sourceFile = Path.Combine(sourceLocation, file);
                var filePathWithGLTFExtentsion = Path.ChangeExtension(file, ".glb");
                var sourceFilePath = Path.Combine(destinationLocation, filePathWithGLTFExtentsion);
                sourceFiles.Add(sourceFilePath);
            }

            // Model Count for Progress bar
            var totalModels = files.Count();
            var uploaded = 0;

            // Create a thread to monitor the upload progress
            /*
            Task uploadProgressThread = Task.Run(() =>
            {
                bool allCompleted = true;
                int completedCount = 0;
                do
                {
                    allCompleted = true;
                    // Needed to update the progress bar
                    completedCount = 0;
                    foreach (Task task in tasks)
                    {
                        if (task.Status == TaskStatus.Running || task.Status == TaskStatus.Created)
                            allCompleted = false;
                        else if (task.Status == TaskStatus.RanToCompletion)
                            completedCount++;
                    }

                    // Send message to UI to update progress bar
                    RemainingModels?.Invoke((totalModels).ToString());

                }
                while (allCompleted && completedCount == totalModels);
            });*/


            // Create separate threads to upload files on the AWS.
            foreach (var currentFile in sourceFiles)
            {
                Task uploadThread = Task.Run(() =>
                {
                    string filename = Path.GetFileName(currentFile);
                    var product = new Products
                    {
                        Code = Path.GetFileNameWithoutExtension(currentFile),
                        ModelURL = uploader.UploadFile(currentFile, filename),
                        ProductParameters = null
                    };

                    productUrls.Add(product);
                    uploaded++;
                    RemainingModels?.Invoke((totalModels-uploaded).ToString());
                });

                tasks.Add(uploadThread);
            }
            Task.WaitAll(tasks.ToArray());
            // Return list of all the urls for the uploaded objects
            return productUrls;
        }
    }
}
