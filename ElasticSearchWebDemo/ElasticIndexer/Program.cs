using DataAccess;
using DataAccess.DataRetrieval;
using DataAccess.Models;
using Nest;
using System;
using System.Linq;
using System.Threading;

namespace ElasticIndexer
{
    class Program
    {
        private static ElasticClient Client { get; set; }
        private static MovieDumpReader DumpReader { get; set; }
        private static string CurrentIndexName { get; set; }

        static void Main(string[] args)
        {
            Client = SearchConfiguration.GetClient();
            DumpReader = new MovieDumpReader();
            CurrentIndexName = SearchConfiguration.CreateIndexName();

            CreateIndex();
            IndexDumps();
            SwapAlias();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void CreateIndex()
        {
            Client.CreateIndex(CurrentIndexName, i => i
                .Settings(s => s
                    .NumberOfShards(2)
                    .NumberOfReplicas(0)
                    )
                .Mappings(m => m
                    .Map<Movie>(map => map
                        .AutoMap()
                        .Properties(ps => ps
                            .Nested<Actor>(n => n
                                .Name(p => p.Cast.First())
                                .AutoMap()
                                )
                            )
                        )
                    )
                );
        }

        static void IndexDumps()
        {
            var movies = DumpReader.Movies;

            Console.Write("Indexing documents into Elasticsearch...");
            var waitHandle = new CountdownEvent(1);

            var bulkAll = Client.BulkAll(movies, b => b
                .Index(CurrentIndexName)
                .BackOffRetries(2)
                .BackOffTime("30s")
                .RefreshOnCompleted(true)
                .MaxDegreeOfParallelism(4)
                .Size(1000)
            );

            bulkAll.Subscribe(new BulkAllObserver(
                onNext: b => Console.Write("."),
                onError: e => throw e,
                onCompleted: () => waitHandle.Signal()
            ));

            waitHandle.Wait();

            Console.WriteLine("Done.");
        }

        private static void SwapAlias()
        {
            var indexExists = Client.IndexExists(SearchConfiguration.LiveIndexAlias).Exists;

            Client.Alias(aliases =>
            {
                if (indexExists)
                    aliases.Add(a => a.Alias(SearchConfiguration.OldIndexAlias).Index(Client.GetIndicesPointingToAlias(SearchConfiguration.LiveIndexAlias).First()));

                return aliases
                    .Remove(a => a.Alias(SearchConfiguration.LiveIndexAlias).Index("*"))
                    .Add(a => a.Alias(SearchConfiguration.LiveIndexAlias).Index(CurrentIndexName));
            });

            var oldIndices = Client.GetIndicesPointingToAlias(SearchConfiguration.OldIndexAlias)
                .OrderByDescending(name => name)
                .Skip(2);

            foreach (var oldIndex in oldIndices)
                Client.DeleteIndex(oldIndex);
        }
    }
}
