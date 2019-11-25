using DataAccess.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DataAccess
{
    public static class SearchConfiguration
    {
        public static ElasticClient GetClient() => new ElasticClient(_connectionSettings);

        static SearchConfiguration()
        {
            _connectionSettings = new ConnectionSettings(CreateUri(9200))
                                                        .DefaultIndex("movieindex")
                                                        .DefaultMappingFor<Movie>(i => i
                                                            .TypeName("movie")
                                                            .IndexName("movieindex")
                                                        );
        }

        private static readonly ConnectionSettings _connectionSettings;

        public static string LiveIndexAlias => "movieindex";

        public static string OldIndexAlias => "movieindex-old";

        public static Uri CreateUri(int port)
        {
            var host = Process.GetProcessesByName("fiddler").Any()
                ? "ipv4.fiddler"
                : "localhost";

            return new Uri($"http://{host}:{port}");
        }

        public static string CreateIndexName() => $"{LiveIndexAlias}-{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss}";

    }
}
