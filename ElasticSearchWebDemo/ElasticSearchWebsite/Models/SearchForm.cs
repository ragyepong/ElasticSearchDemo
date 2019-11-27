using System;

namespace ElasticSearchWebsite.Models
{
    public enum SearchSort
    {
        Relevance,
        Rating,
        Recent
    }

    public class SearchForm
    {
        public const int DefaultPageSize = 10;
        public const int DefaultPage = 1;
        public const SearchSort DefaultSort = SearchSort.Relevance;

        public int Page { get; set; }
        public bool Significance { get; set; }
        public string Query { get; set; }
        public string Genre { get; set; }
        public string[] Tags { get; set; }
        public int PageSize { get; set; }
        public SearchSort Sort { get; set; }

        public SearchForm()
        {
            this.PageSize = DefaultPageSize;
            this.Page = DefaultPage;
            this.Sort = DefaultSort;
            this.Tags = Array.Empty<string>();
        }

        public SearchForm Clone() => new SearchForm
        {
            Page = this.Page,
            Significance = this.Significance,
            Query = this.Query,
            Genre = this.Genre,
            Tags = this.Tags,
            PageSize = this.PageSize,
            Sort = this.Sort,
        };
    }
}
