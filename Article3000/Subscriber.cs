using System;
using System.Collections.Generic;
using System.Linq;

namespace Article3000
{
    public sealed class Subscriber<TData>:ISubscriber<TData>
    {
        private readonly List<Article<TData>> _articles;
        public string Nickname { get; }
        public HashSet<string> Tags { get; }

        public Subscriber()
        {
            _articles = new List<Article<TData>>();
            Tags = new HashSet<string>();
        }
        public Subscriber(string nickname, HashSet<string> tags)
        {
            Nickname = nickname;
            
            _articles = new List<Article<TData>>();
            Tags = new HashSet<string>();
            Tags = tags;
        }

        public void Recieve(object obj,Article<TData> article)
        {
            var a = Console.BufferWidth;
            var b = Console.BufferWidth * 0.1;
            a =(int)(a - b);
            string output ="____________________________________________________________________________________________________________\n" +
                $"{Nickname} ({string.Join(" | ", Tags.ToArray())}) recieved an article {article.ToOptimalString(a)}\n";
            
            _articles.Add(article);
            Console.ResetColor();
            Console.WriteLine(output);
        }
    }
}
