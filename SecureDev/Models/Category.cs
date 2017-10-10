using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Vladi2.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public bool isValidCategory()
        {
            Regex regex;
            Match match;

            this.CategoryName = Sanitizer.GetSafeHtmlFragment(this.CategoryName);
            if (String.IsNullOrEmpty(this.CategoryName))
            {
                return false;
            }

            regex = new Regex(@"^[a-zA-Z]{1,10}[-& ]{0,1}[a-zA-Z]{1,10}$");

            match = regex.Match(this.CategoryName);

            if (!match.Success)
                return false;

            return true;
        }
    }

    public class CategoryVM
    {
        public List<Category> categories { get; set; }
        public Category newCategory { get; set; }
    }
}