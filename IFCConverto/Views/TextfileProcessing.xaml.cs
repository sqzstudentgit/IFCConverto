using IFCConverto.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;

namespace IFCConverto.Views
{
    /// <summary>
    /// Interaction logic for TextfileProcessing.xaml
    /// </summary>
    public partial class TextfileProcessing : Page
    {
        public TextfileProcessingViewModel TextfileProcessingViewModel { get; set; }

        public TextfileProcessing()
        {
            InitializeComponent();
            TextfileProcessingViewModel = new TextfileProcessingViewModel(DialogCoordinator.Instance);
            DataContext = TextfileProcessingViewModel;
        }
    }
}
