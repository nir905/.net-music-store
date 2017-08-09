using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vladi2.Models
{
    public class Disc
    {
        public int DiscID { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public DateTime DiscAdded { get; set; }
        public Category Category { get; set; }
        public string PictureUrl { get; set; }
        public float price { get; set; }
        public string Duration { get; set; }
        public int SongsAmount { get; set; }
    }
}