using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoviesBroker.Models.ExternalModels
{
    public class FlickrSearchResponse
    {
        public PhotoColection Photos { get; set; }
        public string Stat { get; set; }
    }

    public class PhotoColection
    {
        public int Page { get; set; }
        public int Pages { get; set; }
        public int PerPage { get; set; }
        public int Total { get; set; }

        public Photo[] Photo { get; set; }   
    }

    public class Photo
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public string Secret { get; set; }
        public string Server { get; set; }
        public string Title { get; set; }
        public int IsPublic { get; set; }
        public int IsFriend { get; set; }
        public int IsFamily { get; set; }
    }

}