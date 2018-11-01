using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Article3000.Database;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Article3000
{
    public class Publisher:IPublisher<string>
    {
        private static readonly Random Rnd = new Random();
        private static bool isAllowInternetUsage;
        private CancellationTokenSource _pCancellationTokenSource;
        private Task _publishingTask;

        static Publisher()
        {
            isAllowInternetUsage = true;
        }

        public Publisher( bool startSpam = true)
        {
            
            if (startSpam)
                StartPublishing();
        }

        private void Publishing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var rndArticle = (100000 + Rnd.Next(300000)).ToString();
                var article = GetArticleInfo(rndArticle);
                if (article != null)
                    Released?.Invoke(this, article);
            }
        }

        public void StartPublishing()
        {
            if (_publishingTask != null && !_publishingTask.IsCompleted) return;
            _pCancellationTokenSource = new CancellationTokenSource();
            _publishingTask = new Task(() => Publishing(_pCancellationTokenSource.Token));
            _publishingTask.Start();
        }
        public void StopPublishing()
        {
            _pCancellationTokenSource.Cancel();
        }

        public static Article<string> GetArticleInfo(string id)
        {
            Stopwatch sw= new Stopwatch();
            sw.Start();
            Article<string> article = null;
            try
            {
                var url = "https://habrahabr.ru/post/" + id;
                article =  DatabaseHandlerEntityFramework<string>.GetArticleFromDatabase(id);
                if (article != null)
                {
                    sw.Stop();
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"https://habrahabr.ru/post/{id} Found in DB ({sw.ElapsedMilliseconds} ms.)".PadRight(60));
                    Console.ResetColor();
                    return article;
                }
                if (!isAllowInternetUsage)
                    return null;
                var web = new HtmlWeb();
                var doc = web.Load(url);

                var postNode = doc.DocumentNode.SelectSingleNode("//*[@class=\"post__body post__body_full\"]");

                var data = postNode.SelectSingleNode("//*[@data-io-article-url]").InnerText;

                var title = postNode.SelectSingleNode("//*[@class=\"post__title-text\"]").InnerText;

                var author = postNode.SelectSingleNode("//*[@class=\"user-info__nickname user-info__nickname_small\"]").InnerText;

                var tags = postNode.SelectSingleNode("//*[@class=\"post__tags\"]").InnerText;

                var delimeterChars = new[] { '\n' };
                var splitedTags = tags.Split(delimeterChars, StringSplitOptions.RemoveEmptyEntries);

                var tagsList = new HashSet<string>();

                for (var i = 1; i < splitedTags.Length - 2; ++i)
                {
                    var tag = splitedTags[i].Trim(' ');
                    if (tag.Length != 0)
                        tagsList.Add(tag);
                }
                var date = postNode.SelectSingleNode("//*[@class=\"post__time\"]").InnerText;
                date = CheckDate(date);
                article = new Article<string>(url.Replace("https://habrahabr.ru/post/", ""), title, author, date, data, tagsList);
            }

            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(("https://habrahabr.ru/post/"+id+" 404 Page not found").PadRight(60));
                Console.ResetColor();
                return article;
            }
            DatabaseHandlerEntityFramework<string>.AddTagsToDataBase(article);
            DatabaseHandlerEntityFramework<string>.AddArticleToDatabase(article);
            
            Font font = new Font(FontFamily.GenericMonospace, 20);
            //Article<string>.ToImage(article.ToOptimalString(), font, Color.Black, Color.Cornsilk).Save($@"C:\Users\nnuda\Desktop\HabrArticles\{id}.bmp");
            sw.Stop();
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"https://habrahabr.ru/post/{id} Succeed ({sw.ElapsedMilliseconds} ms.)".PadRight(60));
            Console.ResetColor();

            return article;
        }

        public static string CheckDate(string date)
        {
            //23 января в 22:49
            //21 декабря 2017 в 15:07
            // var a = DateTime.Parse(date.Replace("в ", ""));
            
            var yesterday = DateTime.Today.AddDays(-1);
            date = date.Replace("сегодня ", DateTime.Today.ToLongDateString().Replace("г.", ""));                       
            date = date.Replace("вчера ",yesterday.ToLongDateString().Replace("г.",""));            
            var regex = new Regex(".*20.. в .*", RegexOptions.IgnoreCase);
            if (regex.IsMatch(date))
                return date;
            date = date.Replace(" в ", $" {DateTime.Today.Year.ToString()} в ");
            return date;
        }

        public async Task<Article<string>> GetArticleInfo1(string html)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Article<string> article = await DatabaseHandler<string>.GetArticleFromDatabase(html.Replace("https://habrahabr.ru/post/", ""));
            if (article != null)
            {
                sw.Stop();
                Console.WriteLine("eeeeeee" + sw.ElapsedTicks);

                return article;
            }
            System.Net.WebClient web = new System.Net.WebClient();
            web.Encoding = Encoding.UTF8;

            string result;
            try
            {
                string str = web.DownloadString(html);
                var doc = new HtmlDocument();
                doc.LoadHtml(str);
                HtmlNode htmlNode = doc.DocumentNode.DescendantNodes().First(n => n.Name.Contains("article"));
                result = htmlNode.InnerText;
            }
            catch (Exception)
            {
                return null;
            }

            var articleLines = new List<string>(result.Split('\n'));

            char[] delimeterChars = { ' ', '\n', '\r', '\t' };
            for (int i = 0; i < articleLines.Count - 1; i++)
            {
                bool isUselessString = articleLines[i].StartsWith("!function") ||
                                       articleLines[i].Trim(delimeterChars) == articleLines[i + 1].Trim(delimeterChars) ||
                                       articleLines[i].Contains("Добавить метки");
                if (isUselessString)
                    articleLines.Remove(articleLines[i--]);
            }
            var author = articleLines[0].Trim();

            var date = articleLines[1].Trim();

            var title = articleLines[2].Trim();

            articleLines.RemoveRange(0, 3);

            var splitedArticle = new List<string>(string.Join("\n", articleLines).Split(new[] { "Метки:" }, StringSplitOptions.RemoveEmptyEntries));

            var data = splitedArticle[0].Trim();

            var tags = splitedArticle[1].Split('\n').ToList();

            var splitedTags = new HashSet<string>();

            foreach (var tag in tags)
            {
                if (tag.Trim(delimeterChars).Length != 0 && tag.Trim(delimeterChars).Length < 50)
                    splitedTags.Add(tag.Trim().ToLower());
            }

            if (splitedTags.Count != 0 && splitedTags.Last().Contains("Добавить метки"))
                splitedTags.Remove(splitedTags.Last());

            article = new Article<string>(html.Replace("https://habrahabr.ru/post/", ""), title, author, date, data, splitedTags);
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
            await DatabaseHandler<string>.AddTagsToDataBase(article);
            await DatabaseHandler<string>.AddArticleToDatabase(article);

            return article;
        }

        public event EventHandler<Article<string>> Released;
    }
}
