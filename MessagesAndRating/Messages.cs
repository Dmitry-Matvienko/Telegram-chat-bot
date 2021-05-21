using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Threading.Tasks;

namespace Telegram_bot.MessagesAndRating
{
    class Messages
    {
        public static void TextMessages(object sender, MessageEventArgs e)
        {
            Task.Run(async () =>
            {
                var message = e.Message;
                long Chat_Id = e.Message.Chat.Id;
                if (message.Text is null)
                {
                    return;
                }

                Dictionary<int, string> ranks = new Dictionary<int, string>()
                {
                 { 60,    "Новичок (lvl 1) 🔰"},
                 { 900,   "Дилетант (lvl 2) 🔰"},
                 { 2000,  "Люмпен-пролетарий (lvl 3) 👷‍♂️"},
                 { 3500,  "Барыга (lvl 4) 😐"},
                 { 5000,  "Бронзовый терпила(lvl 5) 🥉"},
                 { 7000,  "Местный гопник (lvl 6) 🦾"},
                 { 9500,  "Средний класс (lvl 7) 👮🏻‍♂️"},
                 { 12000, "Интеллигенция  (lvl 8) 🎖"},
                 { 15000, "Боярин (lvl 9) 👨🏻‍⚖️"},
                 { 18000, "Серебренный терпила (lvl 10) 🥈"},
                 { 22000, "Золотой терпила (lvl 11) 🥇"},
                 { 26000, "Истинный терпила (lvl 12) 🏅"},
                 { 30000, "Терпилоид (lvl 13) 🔥"},
                 { 35000, "Терпиларожденный (lvl 14) 🏆"}
                }; // Список "званий" для участников

                if (message != null)
                {
                    using (var countMessage = new DataBaseBot())
                    {
                        var UserData = new CountMessageTable()
                        {
                            FirstName = message.From.FirstName,
                            UserId = message.From.Id,
                            ChatId = message.Chat.Id,
                            Counter = 1
                        };

                        try
                        {
                            var check_User = countMessage.CountMessageTables.Single(x => x.UserId == UserData.UserId); // Проверяем, есть ли пользователь в бд
                            check_User.Counter += 1; // если есть, то насчитываем ему +1 сообщение в бд
                            countMessage.SaveChanges();
                            check_User.rank = ranks[check_User.Counter]; // даем новое звание, если количество сообщений совпадает с ключем из словаря
                            await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}), твое звание повышено, теперь ты - *{ranks[check_User.Counter]}*", parseMode: ParseMode.Markdown);
                            countMessage.SaveChanges();
                        }
                        catch (InvalidOperationException)
                        {
                            countMessage.CountMessageTables.Add(UserData);// Записываем пользователя впервые
                            countMessage.SaveChanges();
                        }
                        catch (Exception)
                        { }
                    }
                }

                if (message.Text != null && message.ForwardFrom is null && message.ForwardFromChat is null)
                {
                    using (var SecretWords = new DataBaseBot())
                    {
                        try
                        {
                            var secret = new SecretWordTable()
                            {
                                words = message.Text
                            };

                            var Rate = new RatingTable()
                            {
                                FirstName = message.From.FirstName,
                                UserId = message.From.Id,
                            };

                            String[] DropString = secret.words.Split(new char[] { ' ', '?', '!', '.', ',' }, StringSplitOptions.RemoveEmptyEntries); // удалем вероятные символы для получения слова

                            foreach (var UserString in DropString)
                            {
                                string CaseWord = SecretWords.SecretWordTables.FirstOrDefault(x => x.words == UserString).ToString(); // Возвращаем первое подходящее слово из списка
                                var RemoveSecretWord = SecretWords.SecretWordTables.FirstOrDefault(x => x.words == UserString); // Берем тоже слово для дальнейшего удаления
                                var AddRate = SecretWords.RatingTables.Single(x => x.UserId == Rate.UserId);
                                var RandRate = new Random();
                                int value = RandRate.Next(1, 19);
                                int LastValue = AddRate.rate + value; // Добавляем рейтинг к пользователю, который нашел слово
                                AddRate.rate = LastValue;
                                SecretWords.SaveChanges();
                                await Program.botClient.SendTextMessageAsync(Chat_Id, $"Поздравляю, [{message.From.FirstName}](tg://user?id={message.From.Id}), ты нашел тайное слово и получил *{value}* 🆙 рейтинга\nПродолжай общаться и получай больше рейтинга!", parseMode: ParseMode.Markdown);
                                SecretWords.SecretWordTables.Remove(RemoveSecretWord);
                                SecretWords.SaveChanges();
                            }
                        }
                        catch (Exception) {}
                    }
                }

                try
                {
                    if (message.Text == "/localmessage@terpilla_bot" && Chat_Id != message.From.Id 
                     || message.Text == "/localmessage" && Chat_Id != message.From.Id) // узнаем локальный рейтинг
                    {
                        using (var TopLocalRate = new DataBaseBot())
                        {
                            var sort_count_rate = TopLocalRate.CountMessageTables.AsNoTracking().OrderByDescending(x => x.Counter).Select(x => new { x.FirstName, x.Counter, x.UserId, x.ChatId }).ToList(); // сортируем в порядке спадания
                            int i = 1;
                            var sb = new StringBuilder();
                            foreach (var list in sort_count_rate)
                            {
                                if (list.ChatId == Chat_Id)
                                {
                                    sb.Append($"*{i}.* [{list.FirstName}](tg://user?id={list.UserId}) - *{list.Counter} ✉️ сообщений*\n");
                                    i++;
                                }
                                if (i == 11)
                                {
                                    break;
                                }
                            }
                            await Program.botClient.SendTextMessageAsync(Chat_Id, "_Топ-10 болтунов в чате_ \n\n" + sb.ToString(), parseMode: ParseMode.Markdown);
                        }
                    }

                    if (message.Text.Contains(".топС", StringComparison.CurrentCultureIgnoreCase)) // узнаем глобальный рейтинг
                    {
                        using (var TopMessages = new DataBaseBot())
                        {
                            var sort_counters = TopMessages.CountMessageTables.AsNoTracking().OrderByDescending(x => x.Counter).Select(x => new { x.FirstName, x.Counter, x.UserId }).ToList();
                            int i = 1;
                            var sb2 = new StringBuilder();
                            var sb = new StringBuilder();

                            foreach (var list in sort_counters)
                            {
                                if (i >= 1 && i <= 10)
                                {
                                    sb.Append($"*{i}.* [{list.FirstName}](tg://user?id={list.UserId}) - *{list.Counter} ✉️ сообщений*\n"); // топ-10
                                }

                                if (message.From.Id == list.UserId && i > 10)
                                {
                                    sb2.Append($"[{list.FirstName}](tg://user?id={list.UserId}), ты на *{i}* месте - *{list.Counter} ✉️ сообщений*"); // узнать на каком месте пользователь
                                    break;
                                }
                                i++;
                            }
                            await Program.botClient.SendTextMessageAsync(Chat_Id, $"_Глобальный топ болтунов_\n\n{sb}\n{sb2}", parseMode: ParseMode.Markdown);
                        }
                    }
                }
                catch (Exception) { }
            });

        }
    }
    public class Rate
    {
        public static async void Rating(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long Chat_Id = e.Message.Chat.Id;
            if (message.Text is null)
            {
                return;
            }

            if (message.ReplyToMessage != null && message.Text == "+" && message.ReplyToMessage.From.Id != message.From.Id
             || message.ReplyToMessage != null && message.Text.Contains("спасибо", StringComparison.CurrentCultureIgnoreCase) && message.ReplyToMessage.From.Id != message.From.Id)
            {
                using (var context = new DataBaseBot())
                {
                    var UserData = new RatingTable()
                    {
                        FirstName = message.ReplyToMessage.From.FirstName,
                        UserId = message.ReplyToMessage.From.Id,
                        ChatId = Chat_Id,
                        rate = 1,
                    };

                    try
                    {
                        var Record = context.RatingTables.Single(x => x.UserId == UserData.UserId); // Проверяем, если ли в бд пользователь
                        Record.rate += 1; // увеличиваем значение рейтинга на  1
                        context.SaveChanges();
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id})\nувеличил рейтинг\n[{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) *{Record.rate}* 🆙", parseMode: ParseMode.Markdown);
                    }
                    catch (InvalidOperationException)
                    {
                        context.RatingTables.Add(UserData); // записываем пользователя если его нет в бд, начисляя 1 рейтинг
                        context.SaveChanges();
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id})\nувеличил рейтинг\n[{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) *1* 🆙", parseMode: ParseMode.Markdown);
                    }
                    catch (Exception) {}
                }
            }

            try
            {
                if (message.Text == "/localrate@terpilla_bot" && Chat_Id != message.From.Id || message.Text == "/localrate" && Chat_Id != message.From.Id) // локальный рейтинг(только для людей из чата)
                {
                    using (var TopLocalMessage = new DataBaseBot())
                    {
                        var sort_rate = TopLocalMessage.RatingTables.AsNoTracking().OrderByDescending(x => x.rate).Select(x => new { x.FirstName, x.rate, x.UserId, x.ChatId }).ToList(); // Сортировка по убыванию
                        int i = 1;
                        var sb = new StringBuilder();
                        foreach (var list in sort_rate)
                        {
                            if (list.ChatId == Chat_Id)
                            {
                                sb.Append($"*{i}.* [{list.FirstName}](tg://user?id={list.UserId}) - *{list.rate} 🆙 рейтинга*\n");
                                i++;
                            }
                            if (i == 11)
                            {
                                break;
                            }
                        }
                        await Program.botClient.SendTextMessageAsync(Chat_Id, "_Топ-10 рейтинговых пользователей в чате_\n\n" + sb.ToString(), parseMode: ParseMode.Markdown);
                    }
                }

                if (message.Text.Contains(".топР", StringComparison.CurrentCultureIgnoreCase)) // Рейтинг всех пользователей
                {
                    using (var TopRate = new DataBaseBot())
                    {
                        var sort_rate = TopRate.RatingTables.AsNoTracking().OrderByDescending(x => x.rate).Select(x => new { x.FirstName, x.rate, x.UserId }).ToList();
                        int i = 1;
                        var sb = new StringBuilder();
                        var sb2 = new StringBuilder();

                        foreach (var list in sort_rate)
                        {
                            if (i >= 1 && i <= 10)
                            {
                                sb.Append($"*{i}.* [{list.FirstName}](tg://user?id={list.UserId}) - *{list.rate} 🆙 рейтинга*\n");// топ-10
                            }
                            if (message.From.Id == list.UserId && i > 10)
                            {
                                sb2.Append($"[{list.FirstName}](tg://user?id={list.UserId}), ты на *{i}* месте - *{list.rate} 🆙 рейтинга*"); // узнать на каком месте пользователь
                                break;
                            }
                            i++;
                        }
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"_Глобальный топ рейтинговых пользователей_\n\n{sb}\n{sb2}", parseMode: ParseMode.Markdown);
                    }
                }
            }
            catch (Exception) {}
        }

    }
}
