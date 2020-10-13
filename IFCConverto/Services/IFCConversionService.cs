using IFCConvertoLibrary;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task<IFCConvertStatus> ConvertFiles(string sourceLocation, string destinationLocation)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // get all file names from the source location
                    var allFilenames = Directory.EnumerateFiles(sourceLocation).Select(p => Path.GetFileName(p));

                    //// Get all filenames that have a .txt extension, excluding the extension
                    var files = allFilenames.Where(fn => Path.GetExtension(fn) == ".ifc");

                    if (files == null || files.Count() == 0)
                    {
                        return IFCConvertStatus.NoFiles;
                    }

                    var totalFiles = files.Count();

                    // Send the total count of the files to the viewmodel for update on UI
                    TotalFiles?.Invoke(totalFiles.ToString());
                    
                    foreach (var file in files)
                    {
                        // Source FileName: D:\Study\Uni Melb\Semester 7\Software Project\Research\IFC files\Holyoake diffusers_600_12_with Plenum.ifc
                        // Destination FileName: D:\Study\Uni Melb\Semester 7\Software Project\Research\IFC files\22.glb
                        var sourceFile = Path.Combine(sourceLocation, file);
                        var filePathWithGLTFExtentsion = Path.ChangeExtension(file, ".glb");
                        var destinationFile = Path.Combine(destinationLocation, filePathWithGLTFExtentsion);
                        IFCConvert.Convert(sourceFile, destinationFile);

                        totalFiles--;
                        RemainingFiles?.Invoke((totalFiles).ToString());
                    }

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
