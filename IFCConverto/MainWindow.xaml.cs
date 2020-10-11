using IFCConverto.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace IFCConverto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Windows has its own Navigation Service as well. So to clarify that we are using our own, we have to call it using namespace
            Services.NavigationService.Frame = new Frame();
            Services.NavigationService.Frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            HamburgerMenuControl.Content = Services.NavigationService.Frame;
            //WindowTitleBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF28930D"));
        }
        

        /// <summary>
        /// Event handler for clicking the menu items in the hamburger menu
        /// </summary>       
        private void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {
            var menuItem = e.ClickedItem as MenuItemViewModel;

            if (menuItem != null && menuItem.IsNavigation)
            {
                Services.NavigationService.Navigate(menuItem.NavigationDestination);
            }
        }
        
        // When the Window loads, always navigate to the first item in the Menu. In this case, it is IFC Convert.
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Services.NavigationService.Navigate(new Uri("Views/IFCConvert.xaml", UriKind.RelativeOrAbsolute));
            HamburgerMenuControl.SelectedIndex = 0;
        }
    }
}
