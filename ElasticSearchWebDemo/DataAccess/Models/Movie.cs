using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class Movie
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Summary { get; set; }

        public double Rating { get; set; }

        public string AirDate { get; set; }

        public string Genres { get; set; }

        public virtual ICollection<Actor> Cast { get; set; }
    }
}
