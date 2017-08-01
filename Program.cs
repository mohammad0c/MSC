using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSC;
using MSC.Brute;

namespace InstaLover.Core
{
    class Program
    {
        private static List<string> Servers = new List<string>() {
        "http://ru1media.cf/add?id=",
        "http://us1media.cf/add?id=",
        "http://75.102.21.228/add?id=",
        "http://194.58.115.48/add?id=",
         };
        static void Main(string[] args)
        {
            try {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("InstaLover Core Powered By MSC");
                Console.ResetColor();
                Console.WriteLine("Enter Media link:");
                string url = Console.ReadLine();
                Console.WriteLine("Please wait...");
                string id = GetID(url);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Detected ID: {0}", id);
                Console.ResetColor();
                Console.WriteLine("Enter Count of add like:");
                int count = int.Parse(Console.ReadLine());
                for (int i = 0; i <= count; i++)
                {
                    GetLike(id);
                }
                Console.WriteLine("Thanks to all");
            }
            catch(Exception ex) { Console.WriteLine("ERROR: {0}", ex.Message); }
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
        static string GetID(string url)
        {
            Config config = new Config();
            Requester rer = new Requester();
            rer.logger.OnMessageReceived += Logger_OnMessageReceived;
            config.LoginURL = "https://api.instagram.com/oembed/?url=" + url;
            Token tk = rer.GetToken(new Token { RegexPattern = "\"media_id\": \"(.*?)\", \"provider_name\":" }, config);
            return tk.GrpValue[0];
        }

        static void GetLike(string id)
        {
            Random rd = new Random();
            string server = Servers[rd.Next(0, Servers.Count)];
            Config config = new Config();
            Requester rer = new Requester();
            config.LoginURL = server + id;
            config.AllowAutoRedirect = false;
            config.KeepAlive = true;
            rer.GETData(config);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Like added!");
            Console.ResetColor();
        }

        private static void Logger_OnMessageReceived(object sender, MessageReceivedArge e)
        {
            if (e.log.GetMessage(false) != null)
            {
                Console.WriteLine(e.log.GetMessage());
            }
        }
    }
}
