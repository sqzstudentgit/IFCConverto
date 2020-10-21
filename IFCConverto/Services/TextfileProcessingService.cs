using IFCConverto.Models;
using Microsoft.VisualBasic;
using Microsoft.WindowsAPICodePack.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IFCConverto.Enums.DestinationLocationTypeEnum;
using static IFCConverto.Enums.TextfileProcessingEnum;

namespace IFCConverto.Services
{
    public class TextfileProcessingService
    {
        public delegate void TextfileProcessingUpdateForUIEventHandler(string message);
        public event TextfileProcessingUpdateForUIEventHandler TotalFiles;
        public event TextfileProcessingUpdateForUIEventHandler RemainingFiles;
        public event TextfileProcessingUpdateForUIEventHandler ProcessingException;
        public event TextfileProcessingUpdateForUIEventHandler ColumnMismatch;

        /// <summary>
        /// Method to process the text file and generate JSON files
        /// </summary>
        /// <param name="sourceLocation">Source location path</param>
        /// <param name="destinationLocation">Destination location path</param>
        /// <returns>Textfile processing Status to update the UI</returns>
        public async Task<TextfileProcessingStatus> ProcessFiles(string sourceLocation, string destinationLocation, DestinationLocationType destinationType)
        {
            try
            {
                return await Task.Run(async () =>
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

                    // List for all the products with their key value pairs of data. This list will be serialzied and sent to the API
                    var productList = new List<Products>();

                    // Process each file and convert it 
                    foreach (var file in files)
                    {
                        // Get the path for the source file
                        var sourceFile = Path.Combine(sourceLocation, file);                        

                        // Read all the lines of the file                        
                        var lines = File.ReadAllLines(sourceFile).ToArray();

                        // Final Heading Tokens after being cleaned and formatted would be stored in this list
                        var finalHeadings = ProcessHeaderString(lines[0]);

                        // Loop over rest of the lines apart from the heading line
                        foreach (var line in lines.Skip(1))
                        {
                            // Will contain the values from the CSV file 
                            var content = ProcessData(line);

                            // Time to process the strings and convert to Json
                            // However, if this condition is not satisfied, that means there is something wrong with the file. We need to alert the user.
                            if (content != null && finalHeadings != null && content.Count() > 0 && finalHeadings.Count > 0 && finalHeadings.Count == content.Count)
                            {                                
                                var product = new Products
                                {
                                    ProductParameters = new List<ProductParameters>(),                                    
                                };

                                // Now make the key value pairs of the tokenized strings
                                for (var i = 0; i < finalHeadings.Count; i++)
                                {
                                    // add a check to fill out the product code if the token is "Product Code". Then we need to update the product code
                                    // otherwise, fill out key value pairs
                                    if (finalHeadings[i].ToLowerInvariant().Equals("product code"))
                                    {
                                        product.Code = content[i];
                                    }
                                    else
                                    {
                                        var productParam = new ProductParameters
                                        {
                                            Key = finalHeadings[i],
                                            Value = content[i]
                                        };

                                        product.ProductParameters.Add(productParam);
                                    }                                    
                                }

                                productList.Add(product);
                            }
                            else
                            {
                                ColumnMismatch?.Invoke("A mismatch between heading and content columns occured in file: " + Path.GetFileNameWithoutExtension(file));
                            }
                        }
                        
                        totalFiles--;

                        // Send message to UI to update progress bar
                        RemainingFiles?.Invoke((totalFiles).ToString());
                    }

                    // check user's preference and save or send the file accordingly.
                    if (destinationType == DestinationLocationType.Local)
                    {
                        StoreDataLocally(productList, destinationLocation);
                    }
                    else if (destinationType == DestinationLocationType.Server)
                    {                        
                        var result = await SendDataToAPI(productList);

                        if(!result)
                        {
                            ProcessingException?.Invoke("Could not send data to Server");
                            return TextfileProcessingStatus.Error;
                        }
                    }
                    else if(destinationType == DestinationLocationType.Both)
                    {
                        StoreDataLocally(productList, destinationLocation);
                        var result = await SendDataToAPI(productList);

                        if (!result)
                        {
                            ProcessingException?.Invoke("Could not send data to Server");
                            return TextfileProcessingStatus.Error;
                        }
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

        /// <summary>
        /// This method will store the data locally in the JSON file at the destination location
        /// </summary>
        /// <param name="productList">List of products</param>
        /// <param name="destinationLocation">Destination location path</param>
        private void StoreDataLocally(List<Products> productList, string destinationLocation)
        {
            // Combine all the processed textfile in 1 Json file and place in the destiantion folder
            var destinationFile = Path.Combine(destinationLocation, "ParameterizedData.json");

            using (StreamWriter file = File.CreateText(destinationFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, productList);
            }            
        }

        /// <summary>
        /// This method will post the data to the Server for DB storage
        /// </summary>
        /// <param name="productList">List of products</param>
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

            return await HttpService.Post(productData, serverDetails.ServerURL);
        }

        /// <summary>
        /// This method is used to process the content of the csv file (except the hearder line)
        /// </summary>
        /// <param name="contentString">content of the csv file</param>
        /// <returns></returns>
        private List<string> ProcessData(string contentString)
        {
            List<string> content;

            // Using the built in CSV parser to parse the content as we don't need to clean this like the headers
            using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(contentString)))
            {
                using (Microsoft.VisualBasic.FileIO.TextFieldParser tfp = new Microsoft.VisualBasic.FileIO.TextFieldParser(ms))
                {
                    tfp.Delimiters = new string[] { "," };
                    tfp.HasFieldsEnclosedInQuotes = true;
                    content = tfp.ReadFields().ToList<string>();
                }
            }

            return content;
        }

        /// <summary>
        /// Private method, that will take in the header line and then format and clean it.
        /// This is a custom parser written for the header
        /// </summary>
        /// <param name="headerString">Headings line from the .csv file</param>
        /// <returns></returns>
        private List<string> ProcessHeaderString(string headerString)
        {
            // Lets clean the heading line as it has some noise in it that we don't need
            var cleanHeading = headerString.Replace("##OTHER##", string.Empty).Replace("##", " (").Split(',');
            var finalHeadingList = new List<string>();

            // For each string in this array we need to format them and clean them (look at the text file to understand)
            foreach (var item in cleanHeading)
            {
                var headingSubstrings = item.Split('(');
                var formattedHeading = string.Empty;

                // If the split length is just 1, then it means that there was 
                if (headingSubstrings.Length > 1)
                {
                    // Get the length of the substring array
                    var lastIndex = headingSubstrings.Length;

                    // if the last element is null or empty that means the header is something like Test##Millimeter## (only 1 element for measurement)
                    // else, there are more than 1 element for measuring unit, so go to else part
                    // This part basically removes the '_' and the uncessary '(' introduced before for processing.
                    if (string.IsNullOrEmpty(headingSubstrings[lastIndex - 1]))
                    {
                        headingSubstrings[1] = '(' + headingSubstrings[1].Trim().Replace('_', ' ') + ')';
                    }
                    else
                    {
                        headingSubstrings[1] = '(' + headingSubstrings[1].Replace('_', ' ');
                        headingSubstrings[lastIndex - 1] = headingSubstrings[lastIndex - 1].Replace('(', ' ').Replace('_', ' ') + ')';
                    }
                    
                    // Change the Upper Case words to Title Case
                    for (var i = 1; i < headingSubstrings.Length; i++)
                    {
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                        headingSubstrings[i] = textInfo.ToTitleCase(headingSubstrings[i].ToLower());
                    }

                    // Combine the now cleaned and formatted substring in to 1 complete string
                    foreach (var subString in headingSubstrings)
                    {
                        formattedHeading += subString;
                    }
                }

                // Add the final substrings to the list. It will have formatted and cleaned headings that we can use
                if (string.IsNullOrEmpty(formattedHeading))
                {
                    finalHeadingList.Add(item);
                }
                else
                {
                    finalHeadingList.Add(formattedHeading);
                }                
            }

            return finalHeadingList;
        }
    }
}
