using IFCConvertoLibrary;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Compilation;
using static IFCConverto.Enums.DestinationLocationTypeEnum;
using static IFCConverto.Enums.IFCConvertEnum;

namespace IFCConverto.Services
{
    public class IFCConversionService
    {
        public delegate void ConversionUpdateForUIEventHandler(string message);
        public event ConversionUpdateForUIEventHandler TotalFiles;
        public event ConversionUpdateForUIEventHandler RemainingFiles;
        public event ConversionUpdateForUIEventHandler ConversionException;

        /// <summary>
        /// Method to start converting the ifc files in to the GLTF format
        /// </summary>
        /// <param name="sourceLocation">Source location path</param>
        /// <param name="destinationLocation">Destination location path</param>
        /// <returns>IFC convert Status to update the UI</returns>
        public async Task<IFCConvertStatus> ConvertFiles(string sourceLocation, string destinationLocation, DestinationLocationType destinationType)
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
                        // Create a new method
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
