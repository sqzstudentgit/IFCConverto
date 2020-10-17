using IFCConverto.ViewModels;
using MahApps.Metro.Controls.Dialogs;
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
        }
    }
}
