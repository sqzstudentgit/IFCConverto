using Microsoft.VisualStudio.TestTools.UnitTesting;
using IFCConverto.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IFCConverto.Enums.DestinationLocationTypeEnum;
using static IFCConverto.Enums.TextfileProcessingEnum;
using IFCConverto.Models;
namespace IFCConverto.Services.Tests
{
    [TestClass()]
    public class TextfileProcessingServiceTests
    {
        [TestMethod()]
        public void ProcessFilesTest()
        {   //This is all the correct parameter 
            AppSettings appSettings = new AppSettings();
            appSettings.Username = "user1";
            appSettings.Password = "squizz";
            appSettings.ServerURL = "http://127.0.0.1:5000";
            SettingsService settingsService = new SettingsService();
            settingsService.SaveSettings(appSettings);
            TextfileProcessingService textfileProcessingService = new TextfileProcessingService();
            string input = "E://downloads//Source";
            string target="E://downloads//res";

            Assert.IsTrue(textfileProcessingService.ProcessFiles(input,null,DestinationLocationType.Server).Result==TextfileProcessingStatus.Done);

            //this is the wrong password after changing the password should save the appSetting to make sure this fucntion can get the wrong password 
            //so that it can work correctly in local but go wrong in server
            appSettings.Password = "12345678";
            settingsService.SaveSettings(appSettings);
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.Error);
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, target, DestinationLocationType.Both).Result == TextfileProcessingStatus.PartialSuccess);

            //in this case change the password to the correct again the funcion will append"/metadata/import"automatically. 
            // AS a result the target URL in the next case will be:127.0.0.1:5000/metadata/import/metadata/import, whichi is a wrong one

            appSettings.Password = "squizz";
            appSettings.ServerURL = "http://127.0.0.1:5000/metadata/import";
            settingsService.SaveSettings(appSettings);
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.Error);


            //In this case I will send a non-exist path
            appSettings.Password = "squizz";
            appSettings.ServerURL = "http://127.0.0.1:5000";
            settingsService.SaveSettings(appSettings);
            input = "E://downloads//12345";
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.Error);

            //= In this case I will use a exsiting folder but no file
            input = "E://downloads//res";
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.NoFiles);


            //in this case I use a folder with one file contains error.
            input = "E://downloads//SourceWithError";
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.Error);

        }
    }
}