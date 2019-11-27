using DataAccess.Models;
using ElasticSearchWebsite.Extensions;
using Nest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchWebsite.Models
{
    public class SearchViewModel
    {
        private static readonly ReadOnlyCollection<IHit<Movie>> EmptyHits =
            new ReadOnlyCollection<IHit<Movie>>(new List<IHit<Movie>>());

        /// <summary>
        /// The current state of the form that was submitted
        /// </summary>
        public SearchForm Form { get; set; }

        /// <summary>
        /// The total number of matching results
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// The current page of package results
        /// </summary>
        public IReadOnlyCollection<IHit<Movie>> Hits { get; set; }

        /// <summary>
        /// Returns how long the elasticsearch query took in milliseconds
        /// </summary>
        public int ElapsedMilliseconds { get; set; }

        /// <summary>
        /// Returns the top genres with the most packages
        /// </summary>
        public Dictionary<string, long?> Genres { get; set; }

        /// <summary>
        /// Returns the top tags for the current search
        /// </summary>
        public Dictionary<string, long> Tags { get; set; }

        public SearchViewModel()
        {
            this.Hits = EmptyHits;
            this.Form = new SearchForm();
            this.Genres = new Dictionary<string, long?>();
            this.Tags = new Dictionary<string, long>();
        }

        public string UrlForFirstPage(Action<SearchForm> alter) => this.UrlFor(form =>
        {
            alter(form);
            form.Page = 1;
        });

        public string UrlFor(Action<SearchForm> alter)
        {
            var clone = this.Form.Clone();
            alter(clone);
            return clone.ToQueryString();
        }

        public string UrlFor(Movie movie) 
        {
            string queryStyledTitle = movie.Title.Replace(" ", "+");
            return $"https://www.google.com/search?q={queryStyledTitle}";
        }
    }
}
