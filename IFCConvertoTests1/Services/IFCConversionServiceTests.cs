using Microsoft.VisualStudio.TestTools.UnitTesting;
using IFCConverto.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IFCConverto.Enums.DestinationLocationTypeEnum;
using static IFCConverto.Enums.IFCConvertEnum;

namespace IFCConverto.Services.Tests
{
    [TestClass()]
    public class IFCConversionServiceTests
    {
        [TestMethod()]
        public void ConvertFilesTest()
        {
            IFCConversionService iFCConversionService = new IFCConversionService();
            string input = "E://downloads//Source";
            string target = "E://downloads//res";
            Assert.IsTrue(iFCConversionService.ConvertFiles(input, target, DestinationLocationType.Local).Result== IFCConvertStatus.Done);
            input = "E://downloads//123";
            Assert.IsTrue(iFCConversionService.ConvertFiles(input, target, DestinationLocationType.Local).Result == IFCConvertStatus.Error);
            input = "E://downloads//res";
            Assert.IsTrue(iFCConversionService.ConvertFiles(input, target, DestinationLocationType.Local).Result == IFCConvertStatus.NoFiles);
        }
    }
}