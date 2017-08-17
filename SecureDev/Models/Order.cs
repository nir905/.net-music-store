using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vladi2.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        private DateTime OrderTime { get; set; }
        public Disc Disc{ get; set; }
        public int Amount { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public bool IsBought { get; set; }
        public bool IsChecked { get; set; }
    }
}