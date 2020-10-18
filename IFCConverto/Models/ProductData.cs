using System.Collections.Generic;

namespace IFCConverto.Models
{
    // DTO class for sending data to the API and getting it back
    public class ProductData
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public List<Products> Products { get; set; }

    }
}
