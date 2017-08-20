using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vladi2.Models
{
    public class Topic
    {
        public string Title{ get; set; }
        public User Author { get; set; }
        public List<Comment> Comments { get; set; }
    }
}