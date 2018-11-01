using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Article3000.Database;

namespace Article3000
{
    static class DatabaseHandler<TData>
    {
        private static readonly SqlConnection SqlConnection;
        static readonly string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\nnuda\source\HomeWorks_sem4\Article3000\Article3000\Database\DataBase.mdf;Integrated Security=True;MultipleActiveResultSets=True";
        static DatabaseHandler()
        {

            SqlConnection = new SqlConnection(ConnectionString);
            SqlConnection.Open();
        }

        public static async Task<Article<string>> GetArticleFromDatabase(string Id)
        {
            var sqlCommand = new SqlCommand(@"SELECT * FROM Articles WHERE Id = @Id", SqlConnection);
            sqlCommand.Parameters.AddWithValue("Id",Id);
            SqlDataReader sqlDataReader = null;
            try
            {
                sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                while (await sqlDataReader.ReadAsync())
                {
                    var author = sqlDataReader["Author"].ToString();
                    var date = sqlDataReader["Date"].ToString();
                    var data = sqlDataReader["Data"].ToString();
                    var title = sqlDataReader["Title"].ToString();
                    var tags = sqlDataReader["Tags"].ToString();
                    var article = new Article<string>(Id, title, author, date, data,
                        new HashSet<string>(tags.Split(' ')));
                    return article;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                sqlDataReader?.Close();
            }
            return null;
        }

        public static async Task AddArticleToDatabase(Article<TData> article)
        {
            var sqlCommand = new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM Articles WHERE Id = @Id)
                                BEGIN
                                    INSERT INTO Articles(Id,Author,Date,Title,Data,Tags) VALUES(@Id,@Author,@Date,@Title,@Data,@Tags) 
                                END", SqlConnection);
            try
            {
                sqlCommand.Parameters.AddWithValue("Id", article.Id);
                sqlCommand.Parameters.AddWithValue("Author", article.Author);
                sqlCommand.Parameters.AddWithValue("Date", article.Date);
                sqlCommand.Parameters.AddWithValue("Title", article.Title);
                sqlCommand.Parameters.AddWithValue("Data", article.Data);
                sqlCommand.Parameters.AddWithValue("Tags", string.Join(" | ", article.Tags));
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
               Console.WriteLine(e);
            }
        }

        public static async Task AddTagsToDataBase(Article<TData> article)
        {
            var sw = new Stopwatch();
            sw.Start();
            foreach (var tag in article.Tags)
            {
                var sqlCommand = new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM Tags WHERE Tag = @tag)
                                BEGIN
                                    INSERT INTO Tags(Tag, Number,ArticlesId) VALUES(@tag, 1,@Id) 
                                END
                                ELSE
                                    IF NOT EXISTS(SELECT 1 FROM Articles WHERE Id = @Id)  
                                    UPDATE Tags SET Number = Number + 1, ArticlesId = ArticlesId + ' ' + @Id 
                                        WHERE Tag = @tag AND ArticlesId NOT LIKE '%@Id%'", SqlConnection);

                try
                {
                    sqlCommand.Parameters.AddWithValue("tag", tag);
                    sqlCommand.Parameters.AddWithValue("Id",article.Id);
                    sqlCommand.Parameters.AddWithValue("number", 1);
                    
                    await sqlCommand.ExecuteNonQueryAsync();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        public static List<Article<string>> GetAllArticleWithTags(HashSet<string> tags)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var articles = new HashSet<Article<string>>();
            var sqlCommand = new SqlCommand(@"SELECT * FROM Tags WHERE Tag = @tag", SqlConnection);
            SqlDataReader sqlDataReader = null;
            
            foreach (var tag in tags)
            {
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.AddWithValue("tag", tag);
                try
                {
                    sqlDataReader = sqlCommand.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        var articlesId = sqlDataReader["ArticlesId"].ToString().Split(' ');
                        var gettingArticlesTaskList = new List<Task<Article<string>>>();
                        foreach (var id in articlesId)
                        {
                            var a = new Task<Article<string>>(() => Publisher.GetArticleInfo(id));
                            a.Start();
                            gettingArticlesTaskList.Add(a);
                        }
                      //  Task.WhenAll(GettingArticlesTaskList.ToArray());
                        foreach (var task in gettingArticlesTaskList)
                        {
                            articles.Add(task.Result);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    sqlDataReader?.Close();
                }
            }
            Console.WriteLine("total: "+sw.ElapsedMilliseconds);
            return articles.ToList();
        }

        public static async Task<List<Subscriber<TData>>> GetSubscribersFromDataBase()
        {
           var sw = new Stopwatch();
            sw.Start();
            var subscribers = new List<Subscriber<TData>>();
            SqlDataReader sqlDataReader = null;
            
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Subscribers", SqlConnection);
            
            try
            {
                sqlDataReader =  sqlCommand.ExecuteReader();
                while (await sqlDataReader.ReadAsync())
                {
                    HashSet<string> tags =new HashSet<string>(sqlDataReader["Tags"].ToString().Split(new []{" | "},StringSplitOptions.RemoveEmptyEntries));
                    var nickname = sqlDataReader["Nickname"].ToString();
                    if (nickname.Length == 0)
                        nickname = "Subscriber" + Convert.ToInt32(sqlDataReader["Id"]);

                    var sub = new Subscriber<TData>(nickname,tags);
                    subscribers.Add(sub);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                sqlDataReader?.Close();
            }
            sw.Stop();
            Console.WriteLine("Got subs "+sw.ElapsedMilliseconds);
            return subscribers;
        }

        public static async void AddSubscriberToDatabase(Subscriber<TData> subscriber)
        {
            SqlCommand sqlCommand = new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM Subscribers WHERE Nickname = @Nickname)
                                BEGIN
                                    INSERT INTO Subscribers(Nickname, Tags) VALUES(@Nickname, @Tags) 
                                END", SqlConnection);
            sqlCommand.Parameters.AddWithValue("Tags", string.Join(" | ",subscriber.Tags));

            sqlCommand.Parameters.AddWithValue("Nickname", subscriber.Nickname);
            await sqlCommand.ExecuteNonQueryAsync();
        }

        public static void RemoveSubscriberFromDatabase(Subscriber<TData> subscriber)
        {
            SqlCommand sqlCommand = new SqlCommand(@"DELETE FROM Subscribers WHERE Nickname = @Nickname", SqlConnection);
            sqlCommand.Parameters.AddWithValue("Nickname", subscriber.Nickname);
            sqlCommand.ExecuteNonQuery();
        }

        public static async Task GetRandomTagsFromDataBase()
        {

            SqlDataReader sqlDataReader = null;

            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM [Tags] ORDER BY Number", SqlConnection);
            //SELECT* FROM goods ORDER BY title
            try
            {
                sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                while (await sqlDataReader.ReadAsync())
                {
                    Console.WriteLine(sqlDataReader["Tag"] + " " + sqlDataReader["Number"]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                    sqlDataReader?.Close();
            }
        }



        private static EventHandler Recieved100Articles;
    }
}
