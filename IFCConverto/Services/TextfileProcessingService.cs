using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                        var sourceFile = Path.Combine(sourceLocation, file);                        

                        // Read the first 2 lines of the text file.
                        // We are only read the 2 lines for now due to current requirements, but for future just increase the number in Take(2) to how many lines you want to read
                        var lines = File.ReadLines(sourceFile).Take(2).ToArray();

                        // Lets clean the heading line as it has some noise in it that we don't need
                        var heading = lines[0].Replace("##OTHER##", string.Empty).Replace("##", " (");
                        
                        // obviously this could be improved, but since we are only dealing with first two lines, this will suffice.
                        // Ideally we would take out the header line and put a loop over other lines (if we were reading more than 2)
                        var cleanHeading = heading.Split(',');

                        // Final Heading Tokens after being cleaned and formatted would be stored in this list
                        var finalHeadings = new List<string>();

                        // For each string in this array we need to format them and clean them (look at the text file to understand)
                        foreach (var item in cleanHeading)
                        {
                            var splitHeading = item.Split('(');
                            var finalHeading = string.Empty;

                            // If the split length is just 1, then it means that there was 
                            if (splitHeading.Length > 1)
                            {
                                var lastIndex = splitHeading.Length;
                                splitHeading[1] = '(' + splitHeading[1].Replace('_', ' ');
                                splitHeading[lastIndex - 1] = splitHeading[lastIndex - 1].Replace('(', ' ').Replace('_', ' ') + ')';

                                for(var i = 1; i < splitHeading.Length; i++)
                                {
                                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                                    splitHeading[i] = textInfo.ToTitleCase(splitHeading[i].ToLower());                                    
                                }                                                              

                                foreach (var subString in splitHeading)
                                {
                                    finalHeading += subString;
                                }                                                                  
                            }

                            if(string.IsNullOrEmpty(finalHeading))
                            {
                                finalHeadings.Add(item);
                            }
                            else
                            {
                                finalHeadings.Add(finalHeading);
                            }                            
                        }

                        //var content = lines[1].Split(',');                        
                        string[] output;
                        
                        using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(lines[1])))
                        {
                            using (Microsoft.VisualBasic.FileIO.TextFieldParser tfp = new Microsoft.VisualBasic.FileIO.TextFieldParser(ms))
                            {
                                tfp.Delimiters = new string[] { "," };
                                tfp.HasFieldsEnclosedInQuotes = true;
                                output = tfp.ReadFields();                               
                            }
                        }

                        if (output != null && output.Count() > 0)
                        {
                            var c = output;
                        }

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
