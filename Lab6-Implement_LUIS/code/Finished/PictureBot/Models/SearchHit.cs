using System;
using System.Collections.Generic;

namespace PictureBot.Models
{
    public class SearchHit
    {
        public SearchHit()
        {
            this.PropertyBag = new Dictionary<string, object>();
        }
        public string BlobUri { get; set; }
        public string LocalFilePath { get; set; }
        public string FileName { get; set; }
        public string Caption { get; set; }
        public string[] Tags { get; set; }
        public string Content { get; set; }
        public string language { get; set; }

        public string translated_text { get; set; }

        public string merged_content { get; set; }

        public string[] text { get; set; }
        public string[] imageTags { get; set; }

        public string[] imageCaptions { get; set; }
        public string[] imageCelebrities { get; set; }

        public string Key { get; set; }
        public string Title { get; set; }
        public string PictureUrl { get; set; }
        public string Description { get; set; }
        public IDictionary<string, object> PropertyBag { get; set; }
    }
}