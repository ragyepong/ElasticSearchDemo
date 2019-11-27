using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class Movie
    {
        public int Id { get; set; }

        private string _title;
        public string Title 
        { 
            get { return _title; }
            set 
            {
                _title = value;
                TitleSuggest = new CompletionField
                {
                    Input = new List<string>(Title.Split(' ')) { Title },
                    Weight = Rating != null ? (int?)Rating : null
                };
            } 
        }
        public CompletionField TitleSuggest { get; set; }
        public string Summary { get; set; }

        private double _rating;
        public double Rating 
        { 
            get { return _rating; }
            set 
            {
                _rating = value;
                TitleSuggest.Weight = (int)value;
            } 
        
        }

        public string AirDate { get; set; }

        public string Genres { get; set; }

        public virtual ICollection<Actor> Cast { get; set; }
    }
}
