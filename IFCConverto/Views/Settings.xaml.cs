using IFCConverto.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows.Controls;

namespace IFCConverto.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public SettingsViewModel SettingsViewModel { get; set; }

        public Settings()
        {
            InitializeComponent();
            SettingsViewModel = new SettingsViewModel(DialogCoordinator.Instance);
            DataContext = SettingsViewModel;
            SettingsViewModel.PasswordLoaded += PasswordLoaded;
        }

        /// <summary>
        /// Sets the password in the password box. This breaks the MVVM convention, however, we cannot bind a property directly to the Passwordbox
        /// A longer and complex approach could be to introduced a custom dependency property, but left out for now.
        /// </summary>
        /// <param name="message">Saved Password</param>
        private void PasswordLoaded(string message)
        {
            txtPassword.Password = message;
        }
    }
}
