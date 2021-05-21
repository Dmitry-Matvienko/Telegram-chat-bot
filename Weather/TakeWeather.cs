using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Telegram_bot.Weather
{
    public class temperature
    {
        public double Temp { get; set; }
        public double speed { get; set; }
        public int all { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public int humidity { get; set; }
    }
    public class temperatureResponse
    {
        public temperature Main { get; set; }
        public temperature wind { get; set; }
        public temperature clouds { get; set; }
        public string Name { get; set; }
        public temperature sys { get; set; }
    }
    class TakeWeather
    {
        public static async void FindWeather(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long Chat_Id = e.Message.Chat.Id;
            if (message.Text is null)
            {
                return;
            }

            try
            {
                if (String.Equals(message.Text.Substring(0, message.Text.Length > 6 ? 6 : message.Text.Length), "Погода", StringComparison.CurrentCultureIgnoreCase))
                {
                    String[] NameOfTown = message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string url = $"http://api.openweathermap.org/data/2.5/weather?q={NameOfTown[NameOfTown.Length-1]}&lang=ru&units=metric&appid=fe855fa11f20b26a964257ec1e5b4f49"; // берем последнее слово - название города
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string response1;
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        response1 = streamReader.ReadToEnd();
                    }

                    temperatureResponse temp = JsonConvert.DeserializeObject<temperatureResponse>(response1); // десериализация получаемого ответа JSON
                    var SunRise = DateTimeOffset.FromUnixTimeSeconds(temp.sys.sunrise).DateTime.ToLocalTime(); // рассвет
                    var SunSet = DateTimeOffset.FromUnixTimeSeconds(temp.sys.sunset).DateTime.ToLocalTime(); // закат

                    string degrees = (temp.Main.Temp < 16 && temp.Main.Temp > 0) ? ", уффф прохладно как-то" :
                        ((temp.Main.Temp < 0) ? ", ой холодно как...одевайся теплее :)" : ", хорошая погодка :)");

                    string Humidity = (temp.Main.humidity >= 1 && temp.Main.humidity <= 30) ? ", суховато" :
                        ((temp.Main.humidity > 30 && temp.Main.humidity < 80) ? ", смотри не подскользнись😂" : ", слииишком влажно...");

                    string Cloudiness = temp.clouds.all > 70 ? ", походу скоро дождик" : ", наслаждайся солнцем)";
                    if (temp.Main.Temp > 16)
                    {
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id})\n\n🌡 Погода в {temp.Name}: *{temp.Main.Temp}* °C {degrees}\n\n" +
                            $"💨 Скорость ветра: *{temp.wind.speed}* м/с\n\n☁️ Облачность: *{temp.clouds.all}* %  {Cloudiness}\n\n💦 Влажность: *{temp.Main.humidity}* %  {Humidity}\n\n🌅 Рассвет в {temp.Name} - *{SunRise}* (UTC + 3 часа)" +
                            $"\n\n🌇 Закат в {temp.Name} - *{SunSet}* (UTC + 3 часа)", ParseMode.Markdown);
                    }
                    else if (temp.Main.Temp < 16)
                    {
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id})\n\n🌡 Погода в {temp.Name}: *{temp.Main.Temp}* °C {degrees}\n\n" +
                            $"💨 Скорость ветра: *{temp.wind.speed}* м/с\n\n☁️ Облачность: *{temp.clouds.all}* %  {Cloudiness}\n\n💦 Влажность: *{temp.Main.humidity}* %  {Humidity}\n\n🌅 Рассвет в {temp.Name} - *{SunRise}* (UTC + 3 часа)" +
                            $"\n\n🌇 Закат в {temp.Name} - *{SunSet}* (UTC + 3 часа)", ParseMode.Markdown);
                    }
                }
            }
            catch (WebException ex) when (ex.Message.Contains("404")) { await Program.botClient.SendTextMessageAsync(Chat_Id, $"Если ты хочешь узнать погоду в своем городе, введи его правильное название :)", replyToMessageId: message.MessageId); }
            catch (Exception) {}
        }

    }
}
