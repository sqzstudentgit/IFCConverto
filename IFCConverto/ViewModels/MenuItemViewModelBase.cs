using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCConverto.ViewModels
{
    // base class for declaring the application menu. 
    // For some reason this is the only way it was working with MahApp apps, so had to do it this way as well
    public class MenuItemViewModelBase : ViewModelBase
    {
        private static readonly ObservableCollection<MenuItemViewModel> ApplicationMenu = new ObservableCollection<MenuItemViewModel>();
        private static readonly ObservableCollection<MenuItemViewModel> ApplicationOptionsMenu = new ObservableCollection<MenuItemViewModel>();

        public MenuItemViewModelBase()
        {
        }

        public ObservableCollection<MenuItemViewModel> Menu => ApplicationMenu;
        public ObservableCollection<MenuItemViewModel> OptionMenu => ApplicationOptionsMenu;
    }
}
