using System.Collections.Generic;

namespace IFCConverto.Models
{
    public class Products
    {
        public string Code { get; set; }

        public List<ProductParameters> ProductParameters { get; set; }
    }
}
