using RestSharp;
using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace Bot_hw
{
    class Program
    {
        static TelegramBotClient bot;
        static string tgDir = @"telegram_files";

        static void Main(string[] args)
        {

            
            string token = File.ReadAllText(@"token.txt");
            if (!Directory.Exists(tgDir))
                try
                {
                    Directory.CreateDirectory(tgDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при создании папки: " + ex.Message);
                }
                

            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageOn;
            bot.StartReceiving();
            Console.ReadKey();
        }
        private static async void MessageOn(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string text = $"{DateTime.Now.ToLongTimeString()} | {e.Message.Chat.FirstName} | {e.Message.Chat.Id}: {e.Message.Text} | {e.Message.Type}";
            Console.WriteLine(text);
            
            switch (e.Message.Type.ToString())
            {
                case "Text":
                    string messageText = e.Message.Text.ToLower();
                    ReplyOnText(messageText, e.Message.Chat.Id);
                    break;

                case "Document":
                    DownLoad(e.Message.Document.FileId, $@"{tgDir}\{e.Message.Document.FileName}");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {e.Message.Document.FileName}, сохранен как {e.Message.Document.FileName}");
                    break;

                case "Photo":
                    string namep = "photo" + Guid.NewGuid();
                    DownLoad(e.Message.Photo[e.Message.Photo.Length - 1].FileId, $@"{tgDir}\{namep}.jpg");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {@"C:\photo.jpg"}, сохранен как {namep}.jpg");
                    break;
                case "Audio":
                    string namea = "audio" + Guid.NewGuid();
                    DownLoad(e.Message.Audio.FileId, $@"{tgDir}\{namea}.mp3");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {e.Message.Audio.Title}, сохранен как {namea}.mp3");
                    break;

                case "Video":
                    DownLoad(e.Message.Video.FileId, $@"{tgDir}\{e.Message.Document.FileName}");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {e.Message.Document.FileName}, сохранен как {e.Message.Document.FileName}");
                    break;
                default:
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                   $"Файл неизвестного типа. Не знаю, что с ним делать.");
                    break;
            }

        }

        private static async void ReplyOnText(string text, long chatID)
        {
            string botReply;
            switch (text)
            {
                
                case "/ковид":
                    SendPhoto("coronavirus-5018466_640.jpg", chatID);
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: coronavirus-5018466_640.jpg | Photo");
                    try
                    {
                        //var client = new RestClient("https://api.covid19api.com/world/total");
                        //client.Timeout = -1;
                        //var request = new RestRequest(Method.GET);
                        //IRestResponse response = client.Execute(request);
                        //botReply = 
                        //var o = Newtonsoft.Json.Linq.JObject.Parse(response.Content);
                        //botReply = o.SelectToken("TotalConfirmed").ToString();


                        var client = new RestClient("https://covid-19-data.p.rapidapi.com/totals?format=json");
                        var request = new RestRequest(Method.GET);
                        request.AddHeader("x-rapidapi-host", "covid-19-data.p.rapidapi.com");
                        //request.AddHeader("x-rapidapi-key", "ea329a7981msh2ab841c17cc8757p182745jsn40d831331c52"); //a
                        request.AddHeader("x-rapidapi-key", "d93af22c62msh260c18d52dc8569p147315jsn68b6d709f561"); //k
                        IRestResponse response = client.Execute(request);

                        botReply = "Всего в мире: \n";
                        Newtonsoft.Json.Linq.JArray o = Newtonsoft.Json.Linq.JArray.Parse(response.Content);

                        var confirmed = (string)(o[0]["confirmed"]);
                        //await bot.SendTextMessageAsync(chatID,
                        botReply += $"Подтверждено {confirmed} случаев\n";

                        var recovered = (string)o[0]["recovered"];
                        //await bot.SendTextMessageAsync(e.Message.Chat.Id,
                        botReply += $"Выздоровление {recovered} случаев\n";

                        var critical = (string)o[0]["critical"];
                        //await bot.SendTextMessageAsync(e.Message.Chat.Id,
                        botReply += $"В критическом состоянии {critical} случаев\n";

                        var deaths = (string)o[0]["deaths"];
                        //await bot.SendTextMessageAsync(e.Message.Chat.Id,
                        botReply += $"Смерть {deaths} случаев\n";

                        var lastUpdate = (string)o[0]["lastUpdate"];
                        botReply += $"Обновлено {lastUpdate}";
                        await bot.SendTextMessageAsync(chatID, botReply);
                        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: {botReply} | Text");


                    }
                    catch (Exception)
                    {
                        await bot.SendTextMessageAsync(chatID,
                       $"Ошибка запроса. Covid 19 data center не отвечает.");
                        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: Ошибка запроса. Covid 19 data center не отвечает. | Text");
                    }
                    break;


                case "/файл":
                    SendFile("covid-19-4938932_640.png", chatID);
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: covid-19-4938932_640.png | Document");
                    break;

                default:
                    botReply = "Доступные команды:\n" +
                           "/ковид - получить актуальную информацию о распространении коронавирусной инфекции в мире;\n" +
                           "/файл - получить котика.\n" +
                           "Боту можно отправить файл, картинку, видео и звук.";
                    await bot.SendTextMessageAsync(chatID, botReply);
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: {botReply} | Text");
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

        static async void SendPhoto (string path, long chatId)
        {

            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, path);
                await bot.SendPhotoAsync(chatId, inputOnlineFile);
            }
        }
        static async void SendFile(string path, long chatId)
        {

            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, path);
                await bot.SendDocumentAsync(chatId, inputOnlineFile);
            }
        }
    }
}
