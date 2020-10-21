using IFCConverto.MVVM;
using IFCConverto.Services;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using static IFCConverto.Enums.DestinationLocationTypeEnum;
using static IFCConverto.Enums.IFCConvertEnum;

namespace IFCConverto.ViewModels
{
    public class IFCConvertViewModel : ViewModelBase
    {
        #region Private Fields

        private string sourcePath;
        private string destinationPath;        
        private float remainingFiles;
        private float remainingModels;
        private DestinationLocationType destinationType;
        private bool isDestinationFilePickerVisible;
        private bool isAWSDetailsControlVisible;
        private SettingsService settingsService;
        private string bucketName;
        private string accessKey;
        private string secretKey;

        private ICommand sourceLocationAccessCommand;
        private ICommand destinationLocationAccessCommand;
        private ICommand convertCommand;
        private ICommand radioButtonCommand;
        private IOService ioService;
        private IFCConversionService iFCConversionService;

        #endregion

        #region Properties

        public string SourcePath
        {
            get
            {
                return sourcePath;
            }
            set
            {
                sourcePath = value;
                OnPropertyChanged("SourcePath");
            }
        }

        public string DestinationPath
        {
            get
            {
                return destinationPath;
            }
            set
            {
                destinationPath = value;
                OnPropertyChanged("DestinationPath");
            }
        }

        public string BucketName
        {
            get
            {
                return bucketName;
            }
            set
            {
                bucketName = value;
                OnPropertyChanged("BucketName");
            }
        }

        public string SecretKey
        {
            get
            {
                return secretKey;
            }
            set
            {
                secretKey = value;
                OnPropertyChanged("SecretKey");
            }
        }

        public string AccessKey
        {
            get
            {
                return accessKey;
            }
            set
            {
                accessKey = value;
                OnPropertyChanged("AccessKey");
            }
        }

        public int TotalFiles { get; set; }

        public float RemainingFiles
        {
            get
            {
                return remainingFiles;
            }
            set
            {
                remainingFiles = value;
                OnPropertyChanged("RemainingFiles");
            }
        }

        public float RemainingModels
        {
            get
            {
                return remainingModels;
            }
            set
            {
                remainingModels = value;
                OnPropertyChanged("RemainingModels");
            }
        }

        public DestinationLocationType DestinationType
        {
            get
            {
                return destinationType;
            }
            set
            {
                destinationType = value;
                OnPropertyChanged("DestinationType");
            }
        }

        public bool IsDestinationFilePickerVisible
        {
            get
            {
                return isDestinationFilePickerVisible;
            }
            set
            {
                isDestinationFilePickerVisible = value;
                OnPropertyChanged("IsDestinationFilePickerVisible");
            }
        }

        public bool IsAWSDetailsControlVisible
        {
            get
            {
                return isAWSDetailsControlVisible;
            }
            set
            {
                isAWSDetailsControlVisible = value;
                OnPropertyChanged("IsAWSDetailsControlVisible");
            }
        }

        public IDialogCoordinator IDialogCoordinator { get; set; }

        #endregion

        #region Commands

        public ICommand SourceLocationAccessCommand
        {
            get
            {
                return sourceLocationAccessCommand;
            }

            set
            {
                sourceLocationAccessCommand = (SimpleDelegateCommand)value;
                OnPropertyChanged("SourceLocationAccessCommand");
            }
        }

        public ICommand DestinationLocationAccessCommand
        {
            get
            {
                return destinationLocationAccessCommand;
            }

            set
            {
                destinationLocationAccessCommand = (SimpleDelegateCommand)value;
                OnPropertyChanged("DestinationLocationAccessCommand");
            }
        }

        public ICommand ConvertCommand
        {
            get
            {
                return convertCommand;
            }

            set
            {
                convertCommand = (SimpleDelegateCommand)value;
                OnPropertyChanged("ConvertCommand");
            }
        }

        public ICommand RadioButtonCommand
        {
            get
            {
                return radioButtonCommand;
            }

            set
            {
                radioButtonCommand = (SimpleDelegateCommand)value;
                OnPropertyChanged("RadioButtonCommand");
            }
        }        

        #endregion         

        public IFCConvertViewModel(IDialogCoordinator iDialogCoordinator)
        {
            // Commands
            SourceLocationAccessCommand = new SimpleDelegateCommand(AccessSourceLocation, () => true);
            DestinationLocationAccessCommand = new SimpleDelegateCommand(AccessDestinationLocation, () => true);
            ConvertCommand = new SimpleDelegateCommand(ConvertFiles, () => true);
            RadioButtonCommand = new SimpleDelegateCommand(RadioButtonClick, () => true);

            // Services
            ioService = new IOService();
            iFCConversionService = new IFCConversionService();
            IDialogCoordinator = iDialogCoordinator;
            settingsService = new SettingsService();

            // Assignments
            DestinationType = DestinationLocationType.Local;
            IsDestinationFilePickerVisible = true;
            IsAWSDetailsControlVisible = false;

            // Subscribe to event handlers in the service layer
            iFCConversionService.ConversionException += IFCConversionException;
            iFCConversionService.TotalFiles += IFCTotalFiles;
            iFCConversionService.RemainingFiles += IFCRemainingFiles;
            iFCConversionService.RemainingModels += IFCRemainingModels;
        }

        /// <summary>
        /// This method is used to toggle the controls on the screen based on the selection made
        /// </summary>
        private void RadioButtonClick()
        {
            if (DestinationType == DestinationLocationType.Local)
            {
                IsDestinationFilePickerVisible = true;
                IsAWSDetailsControlVisible = false;
            }
            else if(DestinationType == DestinationLocationType.Both || DestinationType == DestinationLocationType.Server)
            {
                IsDestinationFilePickerVisible = true;
                IsAWSDetailsControlVisible = true;
            }
        }

        /// <summary>
        /// This method is used to calcualte the remianing files and update the progress bar on the view
        /// </summary>
        /// <param name="message">Number of files remaining</param>
        private void IFCRemainingFiles(string message)
        {
            var filesLeft = Int32.Parse(message);
            var filesDone = TotalFiles - filesLeft;
            RemainingFiles = ((filesDone * 100) / TotalFiles) ;
        }

        /// <summary>
        /// This method is used to calcualte the remianing models and update the progress bar on the view
        /// </summary>
        /// <param name="message">Number of files remaining</param>
        private void IFCRemainingModels(string message)
        {
            var filesLeft = Int32.Parse(message);
            var filesDone = TotalFiles - filesLeft;
            RemainingModels = ((filesDone * 100) / TotalFiles);
        }

        /// <summary>
        /// Get's the total number of files we are going to process for this process
        /// </summary>
        /// <param name="message">Total Number of files</param>
        private void IFCTotalFiles(string message)
        {
            TotalFiles = Int32.Parse(message);
        }

        /// <summary>
        /// Exception handler method that will display the exception
        /// </summary>
        /// <param name="message">Exception Message</param>
        private async void IFCConversionException(string message)
        {
            _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "There was an exception while processing: " + message);
            return;
        }

        private async void ConvertFiles()
        {
            var validationResult = await Validate();

            if (!validationResult)
            {
                return;
            }

            var status = await iFCConversionService.ConvertFiles(SourcePath, DestinationPath, BucketName, AccessKey, SecretKey, DestinationType);

            if (status == IFCConvertStatus.NoFiles)
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The source folder does not contain IFC files. Please reselect and try again");
                return;
            }
            else if (status == IFCConvertStatus.Done)
            {
                if (DestinationType == DestinationLocationType.Both)
                {
                    _ = await IDialogCoordinator.ShowMessageAsync(this, "Success", "All the IFC files have been converted and uploaded successfully");
                    return;
                }
                else
                {
                    _ = await IDialogCoordinator.ShowMessageAsync(this, "Success", "All the IFC files have been converted successfully");
                    return;
                }
            }

        }

        /// <summary>
        /// Validates that all the required data is present before processing the files
        /// </summary>
        /// <returns>True or false based on the validation</returns>
        private async Task<bool> Validate()
        {
            if(string.IsNullOrEmpty(SourcePath))
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The source location path has not be specified. Please specify before proceeding");
                return false;
            }

            if(DestinationType == DestinationLocationType.Local && string.IsNullOrEmpty(DestinationPath))
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The destination location path has not be specified. Please specify before proceeding");
                return false;
            }

            // Check if we have complete info for writing to local and sending to server
            if (DestinationType == DestinationLocationType.Server || DestinationType == DestinationLocationType.Both)
            {
                if(string.IsNullOrEmpty(DestinationPath))
                {
                    _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The destination location path has not be specified. Please specify before proceeding");
                    return false;
                }

                var appSettings = settingsService.LoadSettings();

                if (string.IsNullOrEmpty(appSettings.ServerURL) || string.IsNullOrEmpty(appSettings.Username) || string.IsNullOrEmpty(appSettings.Password))
                {
                    _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The Server Information is incomplete. Please verify before proceeding");
                    return false;
                }

                if (string.IsNullOrEmpty(AccessKey) || string.IsNullOrEmpty(SecretKey) || string.IsNullOrEmpty(BucketName))
                {
                    _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "AWS Information is incomplete. Please verify before proceeding");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calls the IOService to open the FolderPicker dialog where the user can select the destination folder
        /// </summary>
        private void AccessDestinationLocation()
        {            
            DestinationPath =  ioService.OpenFolderPicker("Select destination folder", DestinationPath);
        }

        /// <summary>
        /// Calls the IOService to open the FolderPicker dialog where the user can select the source folder
        /// </summary>
        private void AccessSourceLocation()
        {            
            SourcePath = ioService.OpenFolderPicker("Select source folder", SourcePath);
        }
    }
}
