using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Создать бота, позволяющего принимать разные типы файлов, 
            // *Научить бота отправлять выбранный файл в ответ
            Thread task = new Thread(FH2020bot);
            task.Start();

        }

        static void FH2020bot()
        {
            //string token = "1261212208:AAG7WKfRwyzrCEavawmKBMq_nBFY3boJJvw";
            string token = File.ReadAllText(@"C:\Users\kpols\Desktop\ДЗ\БОТ\token.txt");
            WebClient wc = new WebClient() { Encoding = Encoding.UTF8 };
            int update_id = 0;
            string startUrl = $@"https://api.telegram.org/bot{token}/";

            while (true)
            {
                string url = $"{startUrl}getUpdates?offset={update_id}";
                var r = wc.DownloadString(url);
                var msgs = JObject.Parse(r)["result"].ToArray();
                foreach (dynamic msg in msgs)
                {
                    update_id = Convert.ToInt32(msg.update_id) + 1;
                    string userMessage = msg.message.text;
                    string userId = msg.message.from.id;
                    string userFirstName = msg.message.from.first_name;

                    string text = $"{userFirstName} {userId} {userMessage}";
                    Console.WriteLine(text);
                    string responseText;
                    switch (userMessage.ToLower())
                    {
                        case "hi":   responseText = $"Hi, {userFirstName}";
                            break;

                        case "what?":
                            using (var webClient = new System.Net.WebClient())
                            {
                                var json = webClient.DownloadString("https://evilinsult.com/generate_insult.php?lang=en&type=json");
                                Newtonsoft.Json.Linq.JObject o = Newtonsoft.Json.Linq.JObject.Parse(json);
                                var value = (string)o["insult"];
                                responseText = (string)value;
                                // Console.WriteLine(o);
                            }
                            break;
                        //case "чего?":
                        //    using (var webClient = new System.Net.WebClient())
                        //    {
                        //        var json = webClient.DownloadString("https://evilinsult.com/generate_insult.php?lang=ru&type=json");
                        //        Newtonsoft.Json.Linq.JObject o = Newtonsoft.Json.Linq.JObject.Parse(json);
                        //        var value = (string)o["insult"];
                        //        responseText = (string)value;
                        //        // Console.WriteLine(o);
                        //    }
                        //    break;

                        default:
                            char[] array = userMessage.ToCharArray();
                            Array.Reverse(array);
                            responseText = new string(array);
                            Console.WriteLine(responseText);
                            break;
                    }
                  

                    url = $"{startUrl}sendMessage?chat_id={userId}&text={responseText}";

                    wc.DownloadString(url);
                }

                Thread.Sleep(100);

            }
        }

    }


}
