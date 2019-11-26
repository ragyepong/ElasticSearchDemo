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
    public class SearchController : Controller
    {
        private readonly IElasticClient _client;

        public SearchController(IElasticClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IActionResult Index(SearchForm form)
        {
            var result = _client.Search<Movie>(s => s
                .Size(25)
                .Query(q => q
                    .Match(m => m
                        .Field(p => p.Title.Suffix("keyword"))
                        .Boost(1000)
                        .Query(form.Query)
                        ) || q
                    .FunctionScore(fs => fs
                        .MaxBoost(50)
                        .Functions(ff => ff
                            .FieldValueFactor(fvf => fvf
                                .Field(p => p.Rating)
                                .Factor(0.0001)
                                )
                            )
                        .Query(query => query
                            .MultiMatch(m => m
                                .Fields(f => f
                                    .Field(p => p.Title, 1.5) 
                                    .Field(p => p.Summary, 0.8)
                                    .Field(p => p.Genres, 0.5)
                                    )
                                .Operator(Operator.And)
                                .Query(form.Query)
                                )
                            )
                        )
                    )
                );

            var model = new SearchViewModel 
            {
                Hits = result.Hits,
                Total = result.Total,
                Form = form
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
