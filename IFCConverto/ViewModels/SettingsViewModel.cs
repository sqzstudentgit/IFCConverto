using IFCConverto.MVVM;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace IFCConverto.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Private Fields

        private string serverURL;
        private string username;
        private string password;

        private ICommand saveCommand;

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

        #endregion

        #region Constructor        

        public SettingsViewModel(IDialogCoordinator dialogCoordinator)
        {
            IDialogCoordinator = dialogCoordinator;

            SaveCommand = new ParamDelegateCommand<object>(Save, ()=>true);
        }

        /// <summary>
        /// Method called when the save button is clicked on the settings view. Saves the current configuration of the utility
        /// </summary>
        private void Save(object param)
        {
            var passwordBox = param as PasswordBox;
            var password = passwordBox.Password;
        }

        #endregion
    }
}
