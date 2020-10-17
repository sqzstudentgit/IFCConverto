namespace IFCConverto.Helpers
{
    /// <summary>
    /// This helper will be used to retrieve and set the setting in the Settings file
    /// </summary>
    public static class SettingsHelper
    {
        public static string ServerUrl
        {
            get
            {
                string url = Properties.Settings.Default.ServerURL;

                if (string.IsNullOrEmpty(url))
                {
                    return string.Empty;
                }

                return url;
            }
            set
            {
                Properties.Settings.Default.ServerURL = value;
            }
        }

        public static string Username
        {
            get
            {
                string username = Properties.Settings.Default.Username;

                if (string.IsNullOrEmpty(username))
                {
                    return string.Empty;
                }

                return username;
            }
            set
            {
                Properties.Settings.Default.Username = value;
            }
        }

        public static string Password
        {
            get
            {
                string password = Properties.Settings.Default.Password;

                if (string.IsNullOrEmpty(password))
                {
                    return string.Empty;
                }

                return password;
            }
            set
            {
                Properties.Settings.Default.Password = value;
            }
        }

        // Method we need to call to save the settings
        public static void Save()
        {            
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Upgrade();
        }
    }
}
