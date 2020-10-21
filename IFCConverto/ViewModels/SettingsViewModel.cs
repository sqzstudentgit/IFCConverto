using IFCConverto.Models;
using IFCConverto.MVVM;
using IFCConverto.Services;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.XPath;

namespace IFCConverto.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {

        #region Delegate

        public delegate void PasswordLoadedEventHandler(string message);
        public event PasswordLoadedEventHandler PasswordLoaded;

        #endregion

        #region Private Fields

        private string serverURL;
        private string username;
        private SettingsService settingsService;
        private string bucketName;
        private string accessKey;
        private string secretKey;

        private ICommand saveCommand;
        private ICommand loadCommand;

        #endregion

        #region Properties
        public string ServerURL
        {
            get
            {
                return serverURL;
            }
            set
            {
                serverURL = value;
                OnPropertyChanged("ServerURL");
            }
        }

        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                OnPropertyChanged("Username");
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

        public IDialogCoordinator IDialogCoordinator { get; set; }

        #endregion

        #region Commands
        public ICommand SaveCommand
        {
            get
            {
                return saveCommand;
            }

            set
            {
                saveCommand = (ParamDelegateCommand<object>)value;
                OnPropertyChanged("SaveCommand");
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return loadCommand;
            }
            set
            {
                loadCommand = (SimpleDelegateCommand)value;
                OnPropertyChanged("LoadCommand");
            }
        }

        #endregion

        #region Constructor        

        public SettingsViewModel(IDialogCoordinator dialogCoordinator)
        {
            IDialogCoordinator = dialogCoordinator;
            settingsService = new SettingsService();
            SaveCommand = new ParamDelegateCommand<object>(Save, () => true);
            LoadCommand = new SimpleDelegateCommand(Load, () => true);
            settingsService.SettingsException += SettingsExceptionAsync;
        }       

        #endregion

        /// <summary>
        /// Settings Service Exception Handler
        /// </summary>
        /// <param name="message"></param>
        private async void SettingsExceptionAsync(string message)
        {
            _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", message);
            return;
        }

        /// <summary>
        /// Method called when the save button is clicked on the settings view. Saves the current configuration of the utility
        /// </summary>
        private async void Save(object param)
        {
            var validationResult = await Validate(param);

            // Validation failed, so we need to return
            if (!validationResult)
            {
                return;
            }

            // validation was sucessful, so we need to process

            var passwordBox = param as PasswordBox;

            AppSettings appSettings = new AppSettings
            {
                ServerURL = ServerURL,
                Username = Username,
                Password = passwordBox.Password,
                BucketName = BucketName,
                SecretKey = SecretKey,
                AccessKey = AccessKey
            };

            // Save the settings
            var result = settingsService.SaveSettings(appSettings);

            if(result)
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Success", "Settings saved. Please restart application");
            }
        }

        /// <summary>
        /// Settings Page Loaded Command Event Handler
        /// </summary>
        private void Load()
        {
            //return;
            var appSettings = settingsService.LoadSettings();
            ServerURL = appSettings.ServerURL;
            Username = appSettings.Username;
            SecretKey = appSettings.SecretKey;
            BucketName = appSettings.BucketName;
            AccessKey = appSettings.AccessKey;
            PasswordLoaded?.Invoke(appSettings.Password);
        }

        /// <summary>
        /// This method validates the screen to ensure that all the required information has been added to the screen
        /// </summary>
        /// <param name="param">Should be password box for now</param>
        /// <returns>True or False based on validation</returns>
        private async Task<bool> Validate(object param)
        {
            if (string.IsNullOrEmpty(ServerURL))
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "Please Ensure Server URL is provided");
                return false;
            }

            if (string.IsNullOrEmpty(Username))
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "Please Ensure Username is provided");
                return false;
            }

            var passwordBox = param as PasswordBox;

            if (passwordBox == null)
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "Please Ensure Password is provided");
                return false;
            }

            var password = passwordBox.Password;

            if (string.IsNullOrEmpty(password))
            {
                _ = await IDialogCoordinator.ShowMessageAsync(this, "Error", "Please Ensure Password is provided");
                return false;
            }

            // All the validation was successfull.
            return true;
        }
    }
}
