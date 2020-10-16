using IFCConverto.MVVM;
using IFCConverto.Services;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static IFCConverto.Enums.TextfileProcessingEnum;

namespace IFCConverto.ViewModels
{
    public class TextfileProcessingViewModel : ViewModelBase
    {
        #region Private Fields

        private string sourcePath;
        private string destinationPath;
        private float remainingFiles;
        private bool isConvertButtonEnabled;

        private ICommand sourceLocationAccessCommand;
        private ICommand destinationLocationAccessCommand;
        private ICommand processCommand;
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
                sourceLocationAccessCommand = (DelegateCommand)value;
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
                destinationLocationAccessCommand = (DelegateCommand)value;
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
                processCommand = (DelegateCommand)value;
                OnPropertyChanged("ConvertCommand");
            }
        }

        public IDialogCoordinator IDialogCoordinator { get; set; }

        public bool IsConvertButtonEnabled
        {
            get
            {
                return isConvertButtonEnabled;
            }
            set
            {
                IsConvertButtonEnabled = value;
                OnPropertyChanged("IsConvertButtonEnabled");
            }
        }

        #endregion      

        public TextfileProcessingViewModel(IDialogCoordinator iDialogCoordinator)
        {
            SourceLocationAccessCommand = new DelegateCommand(AccessSourceLocation, () => true);
            DestinationLocationAccessCommand = new DelegateCommand(AccessDestinationLocation, () => true);
            ProcessCommand = new DelegateCommand(ProcessFiles, () => true);
            ioService = new IOService();
            IDialogCoordinator = iDialogCoordinator;
            textfileProcessingService = new TextfileProcessingService();

        }

        private void RemainingFilesTextFiles(string message)
        {
            var filesLeft = Int32.Parse(message);
            var filesDone = TotalFiles - filesLeft;
            RemainingFiles = ((filesDone * 100) / TotalFiles);
        }

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
            if (!string.IsNullOrEmpty(SourcePath) && !string.IsNullOrEmpty(DestinationPath))
            {
                // Disable the convert button while processing 
                //IsConvertButtonEnabled = false;

                var status = await textfileProcessingService.ProcessFiles(SourcePath, DestinationPath);

                if (status == TextfileProcessingStatus.NoFiles)
                {
                    _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "The source folder does not contain text files. Please reselect and try again");
                    // IsConvertButtonEnabled = true;
                    return;
                }
                else if (status == TextfileProcessingStatus.Done)
                {
                    _ = await IDialogCoordinator.ShowMessageAsync(this, "Success", "All the text files have been processed successfully");
                    //IsConvertButtonEnabled = true;
                    return;
                }
            }
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
