using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Article3000.Database;

namespace Article3000
{
    static class Program
    {
        private static void Main()
        {
            var mitm=new ManInTheMiddle<string>();
            var tags = new HashSet<string>(){ "javascript" };
            var b = new Subscriber<string>("Boss",new HashSet<string>(){"OOP"});
            //DatabaseHandler<string>.RemoveSubscriberFromDatabase(a);
            //var a = DatabaseHandlerEntityFramework<string>.GetAllArticleWithTags(tags);
            //DatabaseHandlerEntityFramework<string>.CheckDateInArticles();

            foreach (var tag in DatabaseHandlerEntityFramework<string>.GetTopTags())
            {
                Console.WriteLine(tag);
            }

             mitm.AddSubscriberFromDatabase();
            
            for (int i = 0; i < 5; i++)
            { 
                mitm.AddPublisher(new Publisher());
            }

            var c =DatabaseHandlerEntityFramework<string>.GetAllArticleWithTags(DatabaseHandlerEntityFramework<string>.GetTopTags(1).Select(s=>s.ToString().Split()[0]));

            Console.ReadKey();
        }
    }


}
