using IFCConverto.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;

namespace IFCConverto.Views
{
    /// <summary>
    /// Interaction logic for IFCConvert.xaml
    /// </summary>
    public partial class IFCConvert : Page
    {
        public IFCConvertViewModel ViewModel { get; set; }

        public IFCConvert()
        {
            InitializeComponent();
            ViewModel = new IFCConvertViewModel(DialogCoordinator.Instance);
            DataContext = ViewModel;
        }
    }
}
