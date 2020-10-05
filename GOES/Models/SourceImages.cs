using System;
using System.Collections.Generic;
using System.Text;

namespace GOES.Models
{
    public class SourceImages
    {
        public string name { get; set; }
        public string spacecraft { get; set; }
        public int interval { get; set; }
        public float aspect { get; set; }

        
        public SourceImagesURLs url {get;set;}
    }
}
