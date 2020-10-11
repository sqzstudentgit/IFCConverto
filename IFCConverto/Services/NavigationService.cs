using System;
using System.Windows.Controls;

namespace IFCConverto.Services
{
    public static class NavigationService
    {
        private static Frame frame;
        public static Frame Frame
        {
            get 
            {
                return frame; 
            }
            set 
            { 
                frame = value; 
            }
        }

        public static bool Navigate(Uri sourcePageUri)
        {
            if (frame.CurrentSource != sourcePageUri)
            {
                return frame.Navigate(sourcePageUri);
            }

            return true;
        }

        public static bool Navigate(object content)
        {
            if (frame.NavigationService.Content != content)
            {
                return frame.Navigate(content);
            }

            return true;
        }

        public static void GoBack()
        {
            if (frame.CanGoBack)
            {
                frame.GoBack();
            }
        }
    }
}
