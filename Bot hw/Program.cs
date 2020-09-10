using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Telegram.Bot.Types.InputFiles;

namespace Bot_hw
{
    class Program
    {
        static TelegramBotClient bot;

        static void Main(string[] args)
        {

            string token = File.ReadAllText(@"C:\Users\kpols\Desktop\ДЗ\БОТ\token.txt");

            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageOn;
            bot.StartReceiving();
            Console.ReadKey();
        }
        private static void MessageOn(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text} {e.Message.Type}";
           
            switch (e.Message.Type.ToString())
            {
                case "Text":
                    if (e.Message.Text!="Covid")
                    {
                        bot.SendTextMessageAsync(e.Message.Chat.Id, $"Шлите файл");
                    }
                        else
                        {
                        //bot.SendPhotoAsync(e.Message.Chat.Id, "covid-19-4938932_640.png" );
                        //bot.SendTextMessageAsync(e.Message.Chat.Id, $"Пока живы");
                       // InputOnlineFile inputOnlineFile = new InputOnlineFile("https://cdn.pixabay.com/photo/2020/03/17/04/28/covid-19-4938932_1280.png");
                       // bot.SendPhotoAsync(e.Message.Chat.Id, inputOnlineFile);
                        bot.SendPhotoAsync(e.Message.Chat.Id, photo: "https://cdn.pixabay.com/photo/2020/03/17/04/28/covid-19-4938932_1280.png", caption: "covid");


                        //try
                        //{
                        //    var client = new RestClient("https://covid-19-data.p.rapidapi.com/totals?format=json");
                        //    var request = new RestRequest(Method.GET);
                        //    request.AddHeader("x-rapidapi-host", "covid-19-data.p.rapidapi.com");
                        //    request.AddHeader("x-rapidapi-key", "d93af22c62msh260c18d52dc8569p147315jsn68b6d709f561");
                        //    IRestResponse response = client.Execute(request);

                        //    Newtonsoft.Json.Linq.JArray o = Newtonsoft.Json.Linq.JArray.Parse(response.Content);

                        //    var confirmed = (string)o[0]["confirmed"];
                        //    bot.SendTextMessageAsync(e.Message.Chat.Id,
                        //    $"Подтверждено {confirmed} случаев");

                        //    var recovered = (string)o[0]["recovered"];
                        //    bot.SendTextMessageAsync(e.Message.Chat.Id,
                        //    $"Выздоровление {recovered} случаев");

                        //    var critical = (string)o[0]["critical"];
                        //    bot.SendTextMessageAsync(e.Message.Chat.Id,
                        //    $"В критическом состоянии {critical} случаев");

                        //    var deaths = (string)o[0]["deaths"];
                        //    bot.SendTextMessageAsync(e.Message.Chat.Id,
                        //    $"Смерть {deaths} случаев");

                        //    var lastUpdate = (string)o[0]["lastUpdate"];
                        //    bot.SendTextMessageAsync(e.Message.Chat.Id,
                        //    $"Обновлено {lastUpdate} ");
                        //}
                        //catch (Exception)
                        //{
                        //    bot.SendTextMessageAsync(e.Message.Chat.Id,
                        //   $"Ошибка запроса. Covid 19 data center не отвечает.");
                        //}

                    }

                    break;
                case "Document":
                    DownLoad(e.Message.Document.FileId, $@"C:\telegram_files\{e.Message.Document.FileName}");
                    bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {e.Message.Document.FileName} сохранен как {$@"C:\telegram_files\{ e.Message.Document.FileName}"}");
                    break;
                case "Photo":
                    string namep = "photo" + Guid.NewGuid();
                    DownLoad(e.Message.Photo[e.Message.Photo.Length-1].FileId, $@"C:\telegram_files\{namep}.jpg");
                    bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {@"C:\photo.jpg"} сохранен как {namep}.jpg");
                    break;
                case "Audio":
                    string namea = "audio" + Guid.NewGuid();
                    DownLoad(e.Message.Audio.FileId, $@"C:\telegram_files\{namea}.mp3");
                    bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {e.Message.Audio.Title} сохранен как {namea}.mp3"); 
                    break;

                case "Video":
                    DownLoad(e.Message.Video.FileId, $@"C:\telegram_files\{e.Message.Document.FileName}");
                    bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {e.Message.Document.FileName}  сохранен как {e.Message.Document.FileName}");
                    break;
                default:
                    bot.SendTextMessageAsync(e.Message.Chat.Id,
                   $"Получен файл неизвестного типа. Ошибка записи.");
                    break;
            }

        }

        static async void DownLoad(string fileId, string path)
        {
            try
            {
                var file = await bot.GetFileAsync(fileId);
                FileStream fs = new FileStream(path, FileMode.Create);
                await bot.DownloadFileAsync(file.FilePath, fs);
                fs.Close();

                fs.Dispose();

            }
            catch (Exception ex)
            { 
                Console.WriteLine("Ошибка загрузки файла: " + ex.Message); 
            }
        }
    }
}
