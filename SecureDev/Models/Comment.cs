using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vladi2.Models
{
    public class Comment
    {
        public User CommentUser { get; set; }
        public string Text { get; set; }
        public DateTime CommentTime { get; set; }
    }
}