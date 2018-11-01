using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Article3000.Database
{
    static class DatabaseHandlerEntityFramework<TData>
    {
        private static readonly List<Articles> ReleasedArticles;

        private static object Locker;

        static DatabaseHandlerEntityFramework()
        {
            Locker=new object();
            ReleasedArticles=new List<Articles>();
            Recieved100Articles += Add100Articles;
        }

        #region WorkingWitnTags
        public static void AddTagsToDataBase(Article<TData> article)
        {
            using (DataBaseEntities context = new DataBaseEntities())
            {
                foreach (var tag in article.Tags)
                {//IQueryable<Tags> query = context.Tags.Where(c => c.Tag == tag);
                    IQueryable<Tags> query = from c in context.Tags
                        where c.Tag == tag
                        select c;
                    var Tag = query.ToList().FirstOrDefault();
                    if (Tag == null)
                        Tag = new Tags { Number = 1, ArticlesId = article.Id, Tag = tag };
                    else
                    {
                        if (Tag.ArticlesId.Contains(article.Id)) continue;
                        Tag.Number++;
                        Tag.ArticlesId += " " + article.Id;
                    }
                    context.Tags.AddOrUpdate(Tag);
                    context.SaveChanges();
                }
            }
        }

        public static HashSet<Tags> GetTopTags(int count = 10)
        {
            var sw = new Stopwatch();
            sw.Start();
            var topTags = new HashSet<Tags>();
            using (var context = new DataBaseEntities())
            {
                var items = context.Tags.OrderByDescending(u => u.Number).Take(count);
                foreach (var item in items.ToList())
                {
                    topTags.Add(item);
                }
            }
            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds} ms.");
            return topTags;
        }

        public static List<Article<string>> GetAllArticleWithTags(IEnumerable<string> tags)
        {
            var articles = new List<Article<string>>();

            using (var context = new DataBaseEntities())
            {
                foreach (var tag in tags)
                {
                    IQueryable<Tags> query = from c in context.Tags
                        where c.Tag == tag
                        select c;
                    var GettingArticlesTaskList = new List<Task<Article<string>>>();
                    var tagInfo = query.ToList().FirstOrDefault();
                    if (tagInfo != null)
                    {
                        foreach (var articleId in tagInfo.ArticlesId.Split(' '))
                        {
                            var a = new Task<Article<string>>(() => Publisher.GetArticleInfo(articleId));
                            a.Start();
                            GettingArticlesTaskList.Add(a);
                        }
                        Task.WhenAll(GettingArticlesTaskList.ToArray());
                        foreach (var task in GettingArticlesTaskList)
                        {
                            var b = task.Result;
                            if (b != null)
                                articles.Add(task.Result);
                        }
                    }
                }
            }
            return articles;
        }

        #endregion

        #region WorkingWithArticles

        private static void Add100Articles(object obj,EventArgs args)
        {
            var sw = new Stopwatch();
            sw.Start();
            using (DataBaseEntities context = new DataBaseEntities())
            {
                lock (Locker)
                {
                    context.Articles.AddRange(ReleasedArticles);
                    ReleasedArticles.Clear();
                }
                context.SaveChanges();
            }
            sw.Stop();
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"100 Articles has been added ({sw.ElapsedMilliseconds} ms.)".PadRight(60));
            Console.ResetColor();
        }

        public static void AddArticleToDatabase(Article<string> article)
        {
            using (DataBaseEntities context = new DataBaseEntities())
            {
                if (context.Articles.Any(a => a.Id == article.Id))
                    return;
                lock (Locker)
                {
                    ReleasedArticles.Add(new Articles(article));
                    if (ReleasedArticles.Count >= 10)
                        Recieved100Articles?.Invoke(null, null);
                }
            }
        }

        public static Article<string> GetArticleFromDatabase(string id)
        {
            using (DataBaseEntities context = new DataBaseEntities())
            {
                try
                {
                    var article = context.Articles.Find(id);
                    if (article == null) return null;
                    return new Article<string>(article.Id, article.Title, article.Author, article.Data, article.Date, new HashSet<string>(article.Tags.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }
        }

        public static void CheckDateInArticles()
        {
            using (var context = new DataBaseEntities())
            {
                foreach (var article in context.Articles)
                {
                    article.Date = Publisher.CheckDate(article.Date);
                }
                Console.WriteLine($"Made {context.SaveChanges()} changes");
            }
        }
        #endregion

        #region WorkingWithSubscribers
        public static async Task<List<Subscriber<TData>>> GetSubscribersFromDataBase()
        {
            var sw = new Stopwatch();
            sw.Start();
            var subscribers = new List<Subscriber<TData>>();
            using (var context = new DataBaseEntities())
            {
                IQueryable<Subscribers> query = context.Subscribers;

                var subscribersDbData = query.ToList();

                foreach (var subscriber in subscribersDbData)
                {
                    var sub = new Subscriber<TData>(subscriber.Nickname, new HashSet<string>(subscriber.Tags.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries)));
                    subscribers.Add(sub);
                }
            }
            sw.Stop();
            Console.WriteLine($"Got subs ({sw.ElapsedMilliseconds} ms.)");
            return subscribers;
        }

        public static void AddSubscriberToDatabase(Subscriber<string> subscriber)
        {
            using (var context = new DataBaseEntities())
            {
                var sub = new Subscribers
                {
                    Nickname = subscriber.Nickname,
                    Tags = string.Join(" | ", subscriber.Tags)
                };
                if (context.Subscribers.Find(subscriber.Nickname) == null)
                    context.Subscribers.Add(sub);
                context.SaveChanges();
            }
        }

        public static void RemoveSubscriberFromDatabase(Subscriber<string> subscriber)
        {
            using (var context = new DataBaseEntities())
            {
                var sub = new Subscribers
                {
                    Nickname = subscriber.Nickname,
                    Tags = string.Join(" | ", subscriber.Tags)
                };
                if (context.Subscribers.Find(subscriber.Nickname) != null)
                    context.Subscribers.Remove(sub);
                context.SaveChanges();
            }
        }
        #endregion

        public static EventHandler Recieved100Articles;
    }
}
