using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Article3000.Database;

namespace Article3000
{
    class ManInTheMiddle<TData>
    {
        
        private readonly List<ISubscriber<TData>> _subscribers;

        private readonly List<IPublisher<TData>> _publishers;

        private readonly Dictionary<string, EventHandler<Article<TData>>> _tagsEvents;

        public ManInTheMiddle()
        {
            _tagsEvents=new Dictionary<string, EventHandler<Article<TData>>>();
            _subscribers=new List<ISubscriber<TData>>();
            _publishers=new List<IPublisher<TData>>();
        }

        public void AddSubscriber(Subscriber<TData> subscriber)
        {
            //DatabaseHandler<TData>.AddSubscriberToDatabase(subscriber);
            if (!_subscribers.Contains(subscriber))
            _subscribers.Add(subscriber);

            foreach (var tag in subscriber.Tags)
            {
                if (!_tagsEvents.ContainsKey(tag))
                {
                    EventHandler<Article<TData>> eve = null;
                    eve += subscriber.Recieve;
                    _tagsEvents.Add(tag,eve);
                }
                else
                {
                    _tagsEvents[tag] += subscriber.Recieve;
                }
            }
        }

        public void RemoveSubscriber(Subscriber<TData> subscriber)
        {
            //DatabaseHandler<TData>.RemoveSubscriberFromDatabase(subscriber);
            _subscribers.Remove(subscriber);
            foreach (var tag in subscriber.Tags)
            {
                if (_tagsEvents.ContainsKey(tag))
                {
                    _tagsEvents[tag] -= subscriber.Recieve;
                }
            }
        }

        public void AddPublisher(IPublisher<TData> publisher)
        {
            if (!_publishers.Contains(publisher))
            {
                _publishers.Add(publisher);
                publisher.Released += CheckNew1;
            }
        }

        public void RemovePublisher(IPublisher<TData> publisher)
        {
            publisher.Released -= CheckNew1;
            _publishers.Remove(publisher);
        }

        private void CheckNew(object obj, Article<TData> article)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var subscriber in _subscribers)
            {
                foreach (var tag in article.Tags)
                {
                    if (!IsSubscribed(subscriber, tag)) continue;
                    SendArticle(subscriber,article);
                    break;
                }
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }

        private void CheckNew1(object obj, Article<TData> article)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var alreadyGotArticle = new HashSet<Subscriber<TData>>();
            foreach (var tag in article.Tags)
            {
                if (_tagsEvents.ContainsKey(tag))
                {
                    foreach (var subcriber in _tagsEvents[tag].GetInvocationList())
                    {
                        if (!alreadyGotArticle.Contains((Subscriber<TData>) subcriber.Target))
                            subcriber.DynamicInvoke(this, article);
                        alreadyGotArticle.Add((Subscriber<TData>)subcriber.Target);
                    }

                }
            }
            sw.Stop();
        //    Console.WriteLine(sw.ElapsedTicks);
        }

        public async Task AddSubscriberFromDatabase()
        {
            foreach (var sub in await DatabaseHandlerEntityFramework<TData>.GetSubscribersFromDataBase())
            {
                AddSubscriber(sub);
            }
        }

        private bool IsSubscribed(ISubscriber<TData> subscriber, string tag)
        {
            return subscriber.Tags.Contains(tag);
        }


        private void SendArticle(ISubscriber<TData> subscriber, Article<TData> article)
        {
            subscriber.Recieve(this,article);
        }
    }
}
