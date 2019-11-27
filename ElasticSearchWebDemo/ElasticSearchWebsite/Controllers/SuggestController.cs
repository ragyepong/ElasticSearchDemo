using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ElasticSearchWebsite.Models;
using Nest;
using DataAccess.Models;

namespace ElasticSearchWebsite.Controllers
{
    public class SuggestController : Controller
    {
        private readonly IElasticClient _client;

        public SuggestController(IElasticClient client) => _client = client;

        [HttpPost]
        public IActionResult Index([FromBody]SearchForm form)
        {
            var result = _client.Search<Movie>(s => s
                            .Index<Movie>()
                            .Source(sf => sf
                                .Includes(f => f
                                    .Field(ff => ff.Title)
                                    .Field(ff => ff.Rating)
                                    .Field(ff => ff.Summary)
                                )
                            )
                            .Suggest(su => su
                                .Completion("package-suggestions", c => c
                                    .Prefix(form.Query)
                                    .Field(p => p.TitleSuggest)
                                )
                            )
                        );

            var suggestions = result.Suggest["package-suggestions"]
                                .FirstOrDefault()
                                .Options
                                .Select(suggest => new
                                {
                                    title = suggest.Source.Title,
                                    rating = suggest.Source.Rating,
                                    summary = !string.IsNullOrEmpty(suggest.Source.Summary)
                                        ? string.Concat(suggest.Source.Summary.Take(200))
                                        : string.Empty
                                });

            return Json(suggestions);
        }
    }
}
