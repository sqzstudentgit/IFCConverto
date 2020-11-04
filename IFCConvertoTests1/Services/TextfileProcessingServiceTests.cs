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
        {   
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

            appSettings.Password = "12345678";
            settingsService.SaveSettings(appSettings);
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.Error);
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, target, DestinationLocationType.Both).Result == TextfileProcessingStatus.PartialSuccess);

            appSettings.Password = "squizz";
            appSettings.ServerURL = "http://127.0.0.1:5000/metadata/import";
            settingsService.SaveSettings(appSettings);
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.Error);

            appSettings.Password = "squizz";
            appSettings.ServerURL = "http://127.0.0.1:5000";
            settingsService.SaveSettings(appSettings);
            input = "E://downloads//12345";
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.Error);

            input = "E://downloads//res";
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.NoFiles);

            input = "E://downloads//SourceWithError";
            Assert.IsTrue(textfileProcessingService.ProcessFiles(input, null, DestinationLocationType.Server).Result == TextfileProcessingStatus.Error);

        }
    }
}