using IFCConverto.Helpers;
using IFCConverto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCConverto.Services
{
    /// <summary>
    /// This class will be called by the settings view model to process the data entered by the user and save it and also retrieve it when settings page is loaded
    /// </summary>
    public class SettingsService
    {
        public delegate void SettingServiceExceptionEventHandler(string message);
        public event SettingServiceExceptionEventHandler SettingsException;

        /// <summary>
        /// This method will be used to get the app setting from the viewmodel and save it in the App.config file
        /// </summary>
        /// <param name="appSettings">Appsetting object</param>
        /// <returns>bool whether settings were saved successfully or not</returns>
        public bool SaveSettings(AppSettings appSettings)
        {
            try
            {
                SettingsHelper.ServerUrl = appSettings.ServerURL;
                SettingsHelper.Username = EncryptionHelper.EncryptString(appSettings.Username);
                SettingsHelper.Password = EncryptionHelper.EncryptString(appSettings.Password);
                SettingsHelper.Save();
                return true;
            }
            catch(Exception ex)
            {
                SettingsException?.Invoke("There was an exception while saving settings. Exception: " + ex.Message);
                return false;
            }            
        }

        /// <summary>
        /// This method will load the AppSettings (configured by the user) to be displayed on the settings page when it's loaded
        /// </summary>
        /// <returns>Appsetting model</returns>
        public AppSettings LoadSettings()
        {
            try
            {
                return new AppSettings
                {
                    ServerURL = SettingsHelper.ServerUrl,
                    Username = EncryptionHelper.DecryptString(SettingsHelper.Username),
                    Password = EncryptionHelper.DecryptString(SettingsHelper.Password)
                };
            }
            catch(Exception ex)
            {
                SettingsException?.Invoke("There was an exception while loading settings. Exception: " + ex.Message);
                return new AppSettings();
            }
        }
    }
}
