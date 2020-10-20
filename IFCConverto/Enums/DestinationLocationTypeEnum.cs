using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCConverto.Enums
{
    public class DestinationLocationTypeEnum
    {
        // This Enum is binded with the Radio Button in the UI with the help of a convertor in the Convertors folder
        public enum DestinationLocationType
        {
            Local,
            Server,
            Both
        }
    }
}
