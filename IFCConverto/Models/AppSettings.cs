using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCConverto.Models
{
    // Model class for tranferring data between Settings page and settings service
    public class AppSettings
    {
        public string ServerURL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
