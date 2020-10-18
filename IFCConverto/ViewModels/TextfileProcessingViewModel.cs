using IFCConverto.MVVM;
using IFCConverto.Services;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using static IFCConverto.Enums.DestinationLocationTypeEnum;
using static IFCConverto.Enums.TextfileProcessingEnum;

namespace IFCConverto.ViewModels
{
    public class TextfileProcessingViewModel : ViewModelBase
    {
        #region Private Fields

        private string sourcePath;
        private string destinationPath;
        private float remainingFiles;
        private DestinationLocationType destinationType;
        private bool isDestinationFilePickerVisible;
        private SettingsService settingsService;

        private ICommand sourceLocationAccessCommand;
        private ICommand destinationLocationAccessCommand;
        private ICommand processCommand;
        private ICommand radioButtonCommand;
        private IOService ioService;
        private TextfileProcessingService textfileProcessingService;

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

        public ICommand ProcessCommand
        {
            get
            {
                return processCommand;
            }

            set
            {
                processCommand = (SimpleDelegateCommand)value;
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

        public TextfileProcessingViewModel(IDialogCoordinator iDialogCoordinator)
        {
            // Commands            
            SourceLocationAccessCommand = new SimpleDelegateCommand(AccessSourceLocation, () => true);
            DestinationLocationAccessCommand = new SimpleDelegateCommand(AccessDestinationLocation, () => true);
            ProcessCommand = new SimpleDelegateCommand(ProcessFiles, () => true);
            RadioButtonCommand = new SimpleDelegateCommand(RadioButtonClick, () => true);
            
            // Services
            ioService = new IOService();            
            textfileProcessingService = new TextfileProcessingService();
            settingsService = new SettingsService();
            IDialogCoordinator = iDialogCoordinator;

            // Assignments
            DestinationType = DestinationLocationType.Local;
            IsDestinationFilePickerVisible = true;

            // Subscribe to event handlers in the service layer
            textfileProcessingService.ProcessingException += TextfileProcessingException;
            textfileProcessingService.TotalFiles += TotalTextFiles;
            textfileProcessingService.RemainingFiles += RemainingFilesTextFiles;
        }
        
        /// <summary>
        /// This method is used to toggle the controls on the screen based on the selection made
        /// </summary>
        private void RadioButtonClick()
        {
            if(DestinationType == DestinationLocationType.Local || DestinationType == DestinationLocationType.Both)
            {
                IsDestinationFilePickerVisible = true;
            }
            else
            {
                IsDestinationFilePickerVisible = false;
            }            
        }

        /// <summary>
        /// This method is used to calcualte the remianing files and update the progress bar on the view
        /// </summary>
        /// <param name="message">Number of files remaining</param>
        private void RemainingFilesTextFiles(string message)
        {
            var filesLeft = Int32.Parse(message);
            var filesDone = TotalFiles - filesLeft;
            RemainingFiles = ((filesDone * 100) / TotalFiles);
        }

        /// <summary>
        /// Get's the total number of files we are going to process for this process
        /// </summary>
        /// <param name="message">Total Number of files</param>
        private void TotalTextFiles(string message)
        {
            TotalFiles = Int32.Parse(message);
        }

        /// <summary>
        /// Exception handler method that will display the exception
        /// </summary>
        /// <param name="message">Exception Message</param>
        private async void TextfileProcessingException(string message)
        {
            _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "There was an exception while processing: " + message);
            return;
        }

        /// <summary>
        /// Process the textfiles
        /// </summary>
        private async void ProcessFiles()
        {
            var validationResult = await Validate();

            if (!validationResult)
            {
                return;
            }

            var status = await textfileProcessingService.ProcessFiles(SourcePath, DestinationPath, DestinationType);

            if (status == TextfileProcessingStatus.NoFiles)
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The source folder does not contain text files. Please reselect and try again");
                return;
            }
            else if (status == TextfileProcessingStatus.Done)
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Success", "All the text files have been processed successfully");
                return;
            }
        }

        /// <summary>
        /// Validates that all the required data is present before processing the files
        /// </summary>
        /// <returns>True or false based on the validation</returns>
        private async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(SourcePath))
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The source location path has not be specified. Please specify before proceeding");
                return false;
            }

            if ((DestinationType == DestinationLocationType.Local || DestinationType == DestinationLocationType.Both) && string.IsNullOrEmpty(DestinationPath))
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The destination location path has not be specified. Please specify before proceeding");
                return false;
            }

            if ((DestinationType == DestinationLocationType.Server || DestinationType == DestinationLocationType.Both))
            {              
                var appSettings = settingsService.LoadSettings();

                if (string.IsNullOrEmpty(appSettings.ServerURL) || string.IsNullOrEmpty(appSettings.Username) || string.IsNullOrEmpty(appSettings.Password))
                {
                    _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The Server Information is incomplete. Please verify before proceeding");
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
            DestinationPath = ioService.OpenFolderPicker("Select destination folder", DestinationPath);
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
