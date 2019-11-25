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

        static void Main(string[] args)
        {
            Client = SearchConfiguration.GetClient();
            DumpReader = new MovieDumpReader();

            DeleteIndexIfExists();
            CreateIndex();
            IndexDumps();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void DeleteIndexIfExists()
        {
            if (Client.IndexExists("movie").Exists)
                Client.DeleteIndex("movie");

            if (Client.IndexExists("actor").Exists)
                Client.DeleteIndex("actor");

            if (Client.IndexExists("movieindex").Exists)
                Client.DeleteIndex("movieindex");
        }

        static void IndexDumps()
        {
            var movies = DumpReader.Movies;

            Console.Write("Indexing documents into Elasticsearch...");
            var waitHandle = new CountdownEvent(1);

            var bulkAll = Client.BulkAll(movies, b => b
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

        static void CreateIndex()
        {
            Client.CreateIndex("movieindex", i => i
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
    }
}
