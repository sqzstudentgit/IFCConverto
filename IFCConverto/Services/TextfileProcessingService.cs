using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IFCConverto.Enums.TextfileProcessingEnum;

namespace IFCConverto.Services
{
    public class TextfileProcessingService
    {
        public delegate void TextfileProcessingUpdateForUIEventHandler(string message);
        public event TextfileProcessingUpdateForUIEventHandler TotalFiles;
        public event TextfileProcessingUpdateForUIEventHandler RemainingFiles;
        public event TextfileProcessingUpdateForUIEventHandler ProcessingException;

        /// <summary>
        /// Method to process the text file and generate JSON files
        /// </summary>
        /// <param name="sourceLocation">Source location path</param>
        /// <param name="destinationLocation">Destination location path</param>
        /// <returns>Textfile processing Status to update the UI</returns>
        public async Task<TextfileProcessingStatus> ProcessFiles(string sourceLocation, string destinationLocation)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // get all file names from the source location
                    var allFilenames = Directory.EnumerateFiles(sourceLocation).Select(p => Path.GetFileName(p));

                    // Get all filenames that have a .txt extension
                    var files = allFilenames.Where(fn => Path.GetExtension(fn) == ".txt");

                    // If there are no files, then return to notify the user
                    if (files == null || files.Count() == 0)
                    {
                        return TextfileProcessingStatus.NoFiles;
                    }

                    // Get total number of files, (need it for the progress bar)
                    var totalFiles = files.Count();

                    // Send the total count of the files to the viewmodel for update on UI
                    TotalFiles?.Invoke(totalFiles.ToString());

                    // Process each file and convert it 
                    foreach (var file in files)
                    {
                        // We will process each file here.
                        totalFiles--;

                        // Send message to UI to update progress bar
                        RemainingFiles?.Invoke((totalFiles).ToString());
                    }

                    // Return success message
                    return TextfileProcessingStatus.Done;
                });
            }
            catch (Exception ex)
            {
                ProcessingException?.Invoke(ex.Message);
                return TextfileProcessingStatus.Error;
            }
        }
    }
}
