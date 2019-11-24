using Dapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using API_Movie = System.Net.TMDb.Movie;

namespace SampleDataConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new System.Net.TMDb.ServiceClient(ConfigurationManager.AppSettings["TMDB-Api"]))
            {
                for (int i = 1, count = 1000; i <= count; i++)
                {
                    using (IDbConnection dataConnection = new SQLiteConnection(LoadConnectionString()))
                    {
                        var movies = client.Movies.GetTopRatedAsync(null, i, CancellationToken.None).Result;
                        count = movies.PageCount; // keep track of the actual page count

                        int movieCount = 1;

                        foreach (API_Movie m in movies.Results)
                        {
                            Thread.Sleep(250);

                            var movieApi = client.Movies.GetAsync(m.Id, null, true, CancellationToken.None).Result;

                            var movieEntry = dataConnection.Query<Movie>($"select * from Movies where Id = {movieApi.Id}", new DynamicParameters()).FirstOrDefault();

                            if (movieEntry == null)
                            {
                                movieEntry = new Movie();
                                movieEntry.Id = movieApi.Id;
                                movieEntry.Cast = new List<Actor>();
                                movieEntry.Title = movieApi.Title;
                                movieEntry.Summary = movieApi.Overview;
                                movieEntry.AirDate = movieApi.ReleaseDate.HasValue ? movieApi.ReleaseDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                                movieEntry.Rating = (double)movieApi.VoteAverage;
                                movieEntry.Genres = String.Join(", ", movieApi.Genres.Select(genre => genre.Name).ToArray());

                                dataConnection.Execute("insert into Movies (Id, Title, Summary, AirDate, Rating, Genres) values (@Id, @Title, @Summary, @AirDate, @Rating, @Genres)", movieEntry);

                                dataConnection.Open();
                                using (var transaction = dataConnection.BeginTransaction())
                                {
                                    try
                                    {
                                        foreach (var x in movieApi.Credits.Cast.Take(10))
                                        {
                                            Thread.Sleep(250);

                                            var MovieActor = new MovieActor();
                                            MovieActor.Movie_Id = movieEntry.Id;
                                            MovieActor.Actor_Id = GetOrCreateActor(dataConnection, client, x.Id).Id;

                                            dataConnection.Execute("insert into MovieActors (Movie_Id, Actor_Id) values (@Movie_Id, @Actor_Id)", MovieActor);
                                        }

                                        transaction.Commit();
                                    }
                                    catch
                                    {
                                        transaction.Rollback();
                                    }
                                    finally
                                    {
                                        dataConnection.Close();
                                    }
                                }

                                System.Console.WriteLine("[Added] " + movieEntry.Title);
                            }
                            else
                            {
                                System.Console.WriteLine("[Exists] " + movieEntry.Title);
                            }

                            movieCount++;
                            if (movieCount == 5000)
                                break;
                        }
                    }

                }
            }
        }

        private static async Task<Actor> GetOrCreateActor(IDbConnection dataConnection, System.Net.TMDb.ServiceClient client, int id)
        {
            var person = await client.People.GetAsync(id, true, CancellationToken.None);

            var actor = dataConnection.Query<Actor>($"select * from Actors where Id = {id}", new DynamicParameters()).FirstOrDefault();

            if (actor == null)
            {
                actor = new Actor();
                actor.Id = person.Id;
                actor.FullName = person.Name;
                actor.Bio = person.Biography;

                dataConnection.Execute("insert into Actors (Id, FullName, Bio) values (@Id, @FullName, @Bio)", actor);
            }

            return actor;
        }

        private static string LoadConnectionString(string id = "LocalDB")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
