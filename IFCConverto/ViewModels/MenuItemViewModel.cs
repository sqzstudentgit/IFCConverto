using IFCConverto.MVVM;
using System;
using System.Windows.Input;

namespace IFCConverto.ViewModels
{
    /// <summary>
    ///  This view model is binded to the Main Window (for populating the Hamburger Menu Items)
    /// </summary>
    public class MenuItemViewModel : ViewModelBase
    {
        private object icon;
        private string text;
        private DelegateCommand command;
        private Uri navigationDestination;

        public bool IsNavigation => navigationDestination != null;

        public object Icon
        {
            get
            {
                return icon;
            }
            set
            {
                SetProperty(ref icon, value);
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                SetProperty(ref text, value);
            }
        }

        public ICommand Command
        {
            get
            {
                return command;
            }
            set
            {
                SetProperty(ref command, (DelegateCommand)value);
            }
        }

        public Uri NavigationDestination
        {
            get
            {
                return navigationDestination;
            }
            set
            {
                SetProperty(ref navigationDestination, value);
            }
        }
        
    }
}
