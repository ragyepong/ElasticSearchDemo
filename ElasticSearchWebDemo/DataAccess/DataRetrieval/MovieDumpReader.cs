using Dapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace DataAccess.DataRetrieval
{
    public class MovieDumpReader
    {
        public IEnumerable<Movie> Movies { get; set; }

        public MovieDumpReader()
        {
            using (IDbConnection dataConnection = new SQLiteConnection(LoadConnectionString()))
            {
                Movies = dataConnection.Query<Movie>($"select * from Movies", new DynamicParameters());

                foreach (var movie in Movies)
                {
                    movie.Cast = dataConnection.Query<Actor>($"select Id, FullName, Bio from Actors INNER JOIN MovieActors ON Actors.Id = MovieActors.Actor_Id WHERE MovieActors.Movie_Id = {movie.Id}", new DynamicParameters()).AsList();
                }
            }            
        }

        private string LoadConnectionString(string id = "LocalDB")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
