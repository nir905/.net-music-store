using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vladi2.Models
{
    public class AdminVM
    {
        public List<Disc> Discs { get; set; }
        public List<User> Users { get; set; }
        public List<Category> Categories { get; set; }
    }
}