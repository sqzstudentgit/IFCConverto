using IFCConverto.Models;
using IFCConvertoLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public event ConversionUpdateForUIEventHandler ProcessingException;        

        /// <summary>
        /// Method to start converting the ifc files in to the GLTF format
        /// </summary>
        /// <param name="sourceLocation">Source location path</param>
        /// <param name="destinationLocation">Destination location path</param>
        /// <param name="destinationType">Type of destination, such as server, local or both</param>
        /// <returns>IFC convert Status to update the UI</returns>
        public async Task<IFCConvertStatus> ConvertFiles(string sourceLocation, string destinationLocation, DestinationLocationType destinationType)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    // Check if destination folder exist
                    if(!Directory.Exists(destinationLocation))
                    {
                        ProcessingException?.Invoke("The destination folder does not exist. Please specify the correct folder");
                        return IFCConvertStatus.Error;
                    }

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
                        var productList = UploadModels(files, sourceLocation, destinationLocation);

                        if (productList != null)
                        {
                            var result = await SendDataToAPI(productList);
                            
                            if(!result)
                            {
                                return IFCConvertStatus.Error;
                            }
                        }
                        else
                        {
                            return IFCConvertStatus.Error;
                        }
                    }                                       

                    // Return success message
                    return IFCConvertStatus.Done;
                });                
            }
            catch (Exception ex)
            {
                ProcessingException?.Invoke("There was an error while converting the IFC files. Exception: " + ex.Message);
                return IFCConvertStatus.Error;
            }
        }

        /// <summary>
        /// This method will send data to the API for storage in the database
        /// </summary>
        /// <param name="productList">List of products with their image url</param>
        /// <returns></returns>
        private async Task<bool> SendDataToAPI(List<Products> productList)
        {
            try
            {
                var serverDetails = new SettingsService().LoadSettings();

                var productData = new ProductData
                {
                    Username = serverDetails.Username,
                    Password = serverDetails.Password,
                    Products = productList
                };

                var data = JsonConvert.SerializeObject(productData);

                var result = await HttpService.PostModelLinks(productData, serverDetails.ServerURL);

                if (result.Status.ToLowerInvariant().Equals("success"))
                {
                    return true;
                }
                else
                {
                    ProcessingException?.Invoke("Could not save data to Server due to following reason. Reason:" + result.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ProcessingException?.Invoke("There was an error while sending the data to the server. Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// This method will utilize the S3UploadService to upload the converted models to the S3 bucket.
        /// </summary>
        /// <param name="files">List of files that need to be uploaded</param>
        /// <param name="sourceLocation"></param>
        /// <param name="destinationLocation"></param>
        /// <returns>Return list of products or null</returns>
        private List<Products> UploadModels(IEnumerable<string> files, string sourceLocation, string destinationLocation)
        {
            try
            {
                // Get the AWS details from the settings
                var awsDetails = new SettingsService().LoadSettings();               

                // Initialization                        
                var uploader = new S3UploadService(awsDetails.BucketName, awsDetails.AccessKey, awsDetails.SecretKey);
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
              
                // Create separate threads to upload files on the AWS.
                Parallel.ForEach(sourceFiles, currentFile =>
                {
                    // Call a method to see if the file is created at the location or not before accessing it.
                    var fileFound = WaitForFile(currentFile);
                    string filename = Path.GetFileName(currentFile);

                    // If file is found, then start uploading process
                    // else send a message to the user with the issue related to filename.
                    if (fileFound)
                    {                        
                        var product = new Products
                        {
                            Code = Path.GetFileNameWithoutExtension(currentFile),
                            ModelURL = uploader.UploadFile(currentFile, filename),
                            ProductParameters = null
                        };

                        productUrls.Add(product);
                        uploaded++;
                        RemainingModels?.Invoke((totalModels - uploaded).ToString());
                    }
                    else
                    {
                        ProcessingException?.Invoke("The file: " + filename + " could not be found in the destination folder for uploading");
                    }
                });

                // Return list of all the urls for the uploaded objects
                return productUrls;
            }
            catch (Exception ex)
            {
                ProcessingException?.Invoke("There was an exeception while uploading the files to S3 bucket. Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// This method was introduced as the utility was trying to upload the files that were still being created which caused a crash. This method makes sure that the 
        /// files needed to be accesed have been created. If not, then it would just return false
        /// </summary>
        /// <param name="filename">Name of the intended file with full path</param>
        /// <returns>True or false, depending on whether the file was found or not</returns>
        private bool WaitForFile(string filename)
        {
            // We are only going to check 10 times, if the file is present or not. This will give the system 10 seconds to find the converted file            
            for(var i = 0; i < 10; i++)
            {
                // If file is found, then just return true.
                // else make the thread wait by putting it to sleep for 1 second. 
                if(File.Exists(filename))
                {
                    return true;
                }
                else
                {
                    Thread.Sleep(1000);
                }                    
            }

            // After 5 tries, file not found. Return false
            return false;
        }
    }
}
