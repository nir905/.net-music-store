﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vladi2.Models
{
    public class Topic
    {
        public string Title{ get; set; }
        public string Author { get; set; }

        public DateTime TopicTime { get; set; }

        public int TopicID { get; set; }

        public List<Comment> Comments { get; set; }

        public Comment newComment { get; set; }

    }
}