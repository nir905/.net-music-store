using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Security.Application;

namespace Vladi2.Models
{
    public class CartVM
    {
        public List<Order> Orders { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public bool IsValidInformation()
        {
            this.Address = Sanitizer.GetSafeHtmlFragment(this.Address);
            this.City = Sanitizer.GetSafeHtmlFragment(this.City);
            this.Country = Sanitizer.GetSafeHtmlFragment(this.Country);

            Regex regex;
            Match match;

            regex = new Regex(@"[a-zA-Z]{3,15}");
            match = regex.Match(this.Address);
            if (!match.Success)
                return false;

            match = regex.Match(this.City);
            if (!match.Success)
                return false;

            match = regex.Match(this.Country);
            if (!match.Success)
                return false;

            return true;
        }
    }
}