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

        public static string BucketName
        {
            get
            {
                string bucketName = Properties.Settings.Default.BucketName;

                if (string.IsNullOrEmpty(bucketName))
                {
                    return string.Empty;
                }

                return bucketName;
            }
            set
            {
                Properties.Settings.Default.BucketName = value;
            }
        }

        public static string SecretKey
        {
            get
            {
                string secretKey = Properties.Settings.Default.SecretKey;

                if (string.IsNullOrEmpty(secretKey))
                {
                    return string.Empty;
                }

                return secretKey;
            }
            set
            {
                Properties.Settings.Default.SecretKey = value;
            }
        }

        public static string AccessKey
        {
            get
            {
                string accessKey = Properties.Settings.Default.AccessKey;

                if (string.IsNullOrEmpty(accessKey))
                {
                    return string.Empty;
                }

                return accessKey;
            }
            set
            {
                Properties.Settings.Default.AccessKey = value;
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
