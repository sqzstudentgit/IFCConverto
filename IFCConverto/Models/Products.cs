using System.Collections.Generic;

namespace IFCConverto.Models
{
    // Used in creating JSON String for transfering Data
    public class Products
    {
        public string Code { get; set; }

        public List<ProductParameters> ProductParameters { get; set; }
    }
}
