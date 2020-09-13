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
        static readonly string tgDir = @"telegram_files";    //папка для хранения полученных файлов

        static void Main(string[] args)
        {

            string token = File.ReadAllText(@"token.txt");  // токен Телеграмм - в файле с исполняемым кодом программы
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
            Console.WriteLine("Бот запущен. Для выхода нажмите любую клавишу...");
            Console.ReadKey();
        }
       /// <summary>
       /// Получает и обрабатывает запрос пользователя в чате в асинхронном режиме
       /// </summary>
       /// <param name="sender">телеграмм бот</param>
       /// <param name="e">запрос</param>       
        static async void MessageOn(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string text = $"{DateTime.Now.ToLongTimeString()} | {e.Message.Chat.FirstName} | {e.Message.Chat.Id}: {e.Message.Text} | {e.Message.Type}";
            Console.WriteLine(text);
            
            switch (e.Message.Type.ToString())
            {
                case "Text":        // обработка текстового запроса
                    string messageText = e.Message.Text.ToLower();
                    ReplyOnText(messageText, e.Message.Chat.Id);
                    break;

                case "Document":    // получение файла, сохранение в директории, установленной ранее, с именем, полученным от пользователя
                    DownLoad(e.Message.Document.FileId, $@"{tgDir}\{e.Message.Document.FileName}");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {e.Message.Document.FileName}, сохранен как {e.Message.Document.FileName}");
                    break;

                case "Photo":   // получение файла фотографии, сохранение в директории, установленной ранее, с уникальным идентификатором
                    string namep = "photo" + Guid.NewGuid();    
                    DownLoad(e.Message.Photo[e.Message.Photo.Length - 1].FileId, $@"{tgDir}\{namep}.jpg");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {@"C:\photo.jpg"}, сохранен как {namep}.jpg");
                    break;
                case "Audio": // получение аудио файла, сохранение в директории, установленной ранее, с уникальным идентификатором
                    string namea = "audio" + Guid.NewGuid();
                    DownLoad(e.Message.Audio.FileId, $@"{tgDir}\{namea}.mp3");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"Получен файл {e.Message.Audio.Title}, сохранен как {namea}.mp3");
                    break;

                case "Video":   // получение видео файла, сохранение в директории, установленной ранее, с именем, полученным от пользователя
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
        /// <summary>
        /// Обрабатывает текстовый запрос пользователя и отвечает на него в чат
        /// </summary>
        /// <param name="text">текст запроса</param>
        /// <param name="chatID">ID чата</param>
        static async void ReplyOnText(string text, long chatID)
        {
            string botReply;
            switch (text)
            {
                
                case "/ковид":  //обработка запроса на информацию по Covid19: отправка фото и данных с covid19api.com в телеграмм чат
                                //запрос через RapidAPI агрегатор, бесплатный режим с ограничениями по количеству запросов
                    SendPhoto("coronavirus-5018466_640.jpg", chatID);
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: coronavirus-5018466_640.jpg | Photo");
                    try
                    {
                        //var client = new RestClient("https://api.covid19api.com/world/total");
                        //client.Timeout = -1;
                        //var request = new RestRequest(Method.GET);
                        //IRestResponse response = client.Execute(request);
                        //botReply = "Всего в мире: \n";
                        //var o = Newtonsoft.Json.Linq.JObject.Parse(response.Content);
                        //string confirmed = o.SelectToken("TotalConfirmed").ToString();
                        //string deaths = o.SelectToken("TotalDeaths").ToString();
                        //string recovered = o.SelectToken("TotalRecovered").ToString();
                        //botReply += $"подтвержденных случаев  {confirmed}\nсмертей  {deaths}\nвыздоровело  {recovered}";
                        //await bot.SendTextMessageAsync(chatID, botReply);
                        //Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: {botReply} | Text");


                        var client = new RestClient("https://covid-19-data.p.rapidapi.com/totals?format=json");
                        var request = new RestRequest(Method.GET);
                        request.AddHeader("x-rapidapi-host", "covid-19-data.p.rapidapi.com");
                        //request.AddHeader("x-rapidapi-key", "ea329a7981msh2ab841c17cc8757p182745jsn40d831331c52"); //a
                        request.AddHeader("x-rapidapi-key", "d93af22c62msh260c18d52dc8569p147315jsn68b6d709f561"); //k
                        IRestResponse response = client.Execute(request);

                        botReply = "Всего в мире: \n";
                        Newtonsoft.Json.Linq.JArray o = Newtonsoft.Json.Linq.JArray.Parse(response.Content);
                        var confirmed = (string)(o[0]["confirmed"]);
                        var recovered = (string)o[0]["recovered"];
                        var critical = (string)o[0]["critical"];
                        var deaths = (string)o[0]["deaths"];
                        var lastUpdate = (string)o[0]["lastUpdate"];
                        botReply += $"Подтвержденных случаев {confirmed}\nВыздоровeло {recovered}\nВ критическом состоянии {critical}\n" +
                                    $"Умерло {deaths}\nИнформация обновлена {lastUpdate}";

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


                case "/файл":   //отправка файла по запросу
                    SendFile("covid-19-4938932_640.png", chatID);
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: covid-19-4938932_640.png | Document");
                    break;

                default:    // меню 
                    botReply = "Доступные команды:\n" +
                           "/ковид - получить актуальную информацию о распространении коронавирусной инфекции в мире;\n" +
                           "/файл - получить котика.\n" +
                           "Боту можно отправить файл, картинку, видео и звук.";
                    await bot.SendTextMessageAsync(chatID, botReply);
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | Bot | {chatID}: {botReply} | Text");
                    break;
            }
        }


        /// <summary>
        /// Загружает файл из телеграмм чата
        /// </summary>
        /// <param name="fileId">ID файла</param>
        /// <param name="path">путь для сохранения на компьютере</param>
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
        /// <summary>
        /// Загружает фото из телеграмм чата
        /// </summary>
        /// <param name="path">путь для сохранения на компьютере</param>
        /// <param name="chatId">ID чата</param>
        static async void SendPhoto (string path, long chatId)
        {
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, path);
                    await bot.SendPhotoAsync(chatId, inputOnlineFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка загрузки файла: " + ex.Message);
            }

        }
        /// <summary>
        /// Отправляет файл в телеграмм чат
        /// </summary>
        /// <param name="path">путь к файлу на компьютере</param>
        /// <param name="chatId">ID чата</param>
        static async void SendFile (string path, long chatId)
        {
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, path);
                    await bot.SendDocumentAsync(chatId, inputOnlineFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка чтения/отправки файла: " + ex.Message);
            }
           
        }
    }
}
