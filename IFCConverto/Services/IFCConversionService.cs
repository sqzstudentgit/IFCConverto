using IFCConvertoLibrary;
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
                        // Initialization                        
                        var uploader = new S3UploadService(bucket, accesskey, secretkey);
                        var tasks = new List<Task>();
                        var sourceFiles = new List<string>();
                        var awsUrls = new List<string>();

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

                        // Create a thread to monitor the upload progress
                        Task x = Task.Run(() =>
                        {
                            bool allCompleted = true;
                            do
                            {
                                allCompleted = true;
                                // Needed to update the progress bar
                                int completedCount = 0;
                                foreach (Task task in tasks)
                                {
                                    if (task.Status == TaskStatus.Running || task.Status == TaskStatus.Created)
                                        allCompleted = false;
                                    else if (task.Status == TaskStatus.RanToCompletion)
                                        completedCount++;
                                }
                                // Send message to UI to update progress bar
                                RemainingModels?.Invoke((totalFiles).ToString());

                            }
                            while (!allCompleted);
                        });


                        // Create separate threads to upload files on the AWS.
                        foreach (var currentFile in sourceFiles)
                        {
                            Task t = Task.Run(() =>
                            {
                                string filename = Path.GetFileName(currentFile);
                                awsUrls.Add(uploader.UploadFile(currentFile, filename));
                            });

                            tasks.Add(t);
                        }


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
    }
}
