using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Vladi2.Models
{
    public class Comment
    {
        public string CommentUser { get; set; }

        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
        public DateTime CommentTime { get; set; }
    }
}