using System;
using System.Collections.Generic;

namespace UdemyRealWorldUnitTest.Web.Models
{
    public partial class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public int Stock { get; set; }
        public string Color { get; set; }
    }
}
