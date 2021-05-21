using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;
using System.Linq;
using System.Data;

namespace Telegram_bot.RollGame
{
    class RollEventTable
    {
        public int id { get; set; }
        public int Results { get; set; }
        public string FirstName { get; set; }
        public int UserId { get; set; }
        public long ChatIds { get; set; }
    }
    class RollGames
    {
        public static async void RollButtons(object sender, CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Data == ("Ролл 🎲"))
            {
                using (var RollGame = new DataBaseBot())
                {
                    Random cube = new Random();
                    RollEventTable UserData = new RollEventTable()
                    {
                        FirstName = e.CallbackQuery.From.FirstName,
                        UserId = e.CallbackQuery.From.Id,
                        ChatIds = e.CallbackQuery.Message.Chat.Id,
                        Results = cube.Next(1, 100)
                    };
                    try
                    {
                        var CheckCurrentUserId = RollGame.rollEventTables.Single(x => x.ChatIds == UserData.ChatIds && x.UserId == UserData.UserId); // проверка ,есть ли чат и пользователь в бд
                        await Program.botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ты уже бросал😜", showAlert: true);
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("no")) // Если в бд нет пользователя
                    {
                        RollGame.rollEventTables.Add(UserData);
                        RollGame.SaveChanges();
                        var sort = RollGame.rollEventTables.AsNoTracking().OrderByDescending(x => x.Results).Select(x => new { x.FirstName, x.Results, x.ChatIds }).Where(x => x.ChatIds == e.CallbackQuery.Message.Chat.Id);
                        var sb = new StringBuilder();
                        var sb2 = new StringBuilder();
                        int i = 1;

                        foreach (var a in sort)
                        {
                            if (i == 1)
                            { sb.Append($"🥇 {a.FirstName}: {a.Results}\n"); }
                            if (i == 2)
                            { sb.Append($"🥈 {a.FirstName}: {a.Results}\n"); }
                            if (i == 3)
                            { sb.Append($"🥉 {a.FirstName}: {a.Results}\n"); }
                            else if (i > 3)
                            {
                                sb2.Append($"{a.FirstName}: {a.Results}\n");
                            }
                            i++;
                        }

                        var inline = new InlineKeyboardMarkup(new[]
                               {
                            new[]
                            {
                                 InlineKeyboardButton.WithCallbackData("Ролл 🎲"),
                                 InlineKeyboardButton.WithCallbackData("Остановить розыгрыш ⛔️")
                            }
                        });
                        await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, messageId: e.CallbackQuery.Message.MessageId,
                                                         text: $"Результаты розыграша!\n\nПобедители:\n{sb}\nОстальные участники:\n{sb2}", replyMarkup: inline);
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("more"))
                    {
                        RollGame.rollEventTables.RemoveRange(RollGame.rollEventTables.Where(x => x.ChatIds == UserData.ChatIds && x.UserId == UserData.UserId));
                        RollGame.SaveChanges();
                    }
                }
            }

            if (e.CallbackQuery.Data == ("Остановить розыгрыш ⛔️"))
            {
                using (var DeleteAllLines = new DataBaseBot())
                {
                    var StopGame = new RollEventTable()
                    {
                        ChatIds = e.CallbackQuery.Message.Chat.Id
                    };

                    var sort = DeleteAllLines.rollEventTables.AsNoTracking().OrderByDescending(x => x.Results).Select(x => new { x.FirstName, x.Results, x.ChatIds }).Where(x => x.ChatIds == e.CallbackQuery.Message.Chat.Id);
                    var sb = new StringBuilder();
                    var sb2 = new StringBuilder();
                    int i = 1;

                    foreach (var a in sort)
                    {
                        if (i == 1)
                        { sb.Append($"🥇 {a.FirstName}: {a.Results}\n"); }
                        if (i == 2)
                        { sb.Append($"🥈 {a.FirstName}: {a.Results}\n"); }
                        if (i == 3)
                        { sb.Append($"🥉 {a.FirstName}: {a.Results}\n"); }
                        else if (i > 3)
                        {
                            sb2.Append($"{a.FirstName}: {a.Results}\n");
                        }
                        i++;
                    }

                    DeleteAllLines.rollEventTables.RemoveRange(DeleteAllLines.rollEventTables.Where(x => x.ChatIds == StopGame.ChatIds));
                    DeleteAllLines.SaveChanges();
                    await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, messageId: e.CallbackQuery.Message.MessageId,
                                                  text: $"Победители:\n{sb}\nОстальные участники:\n{sb2}\n*Розыгрыш окончен*!\n\n", parseMode: ParseMode.Markdown);
                }

            }
        }

        public static async void StartEvent(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long Chat_Id = e.Message.Chat.Id;

            if (message.Text == "/start_event@terpilla_bot" 
             || message.Text == "/start_event" && Chat_Id != message.From.Id)
            {
                using (var CheckGame = new DataBaseBot())
                {
                    var CurrentGame = new RollEventTable()
                    {
                        ChatIds = Chat_Id
                    };

                    var stopInline = new InlineKeyboardMarkup(new[]
                     {
                            new[]
                            {
                                 InlineKeyboardButton.WithCallbackData("Остановить розыгрыш ⛔️")
                            }
                            });
                    try
                    {
                        CheckGame.rollEventTables.Single(x => x.ChatIds == CurrentGame.ChatIds); // Проверка на наличие чата в бд
                        await Program.botClient.SendTextMessageAsync(Chat_Id, "Для начала нового розыграша, завершите старый!", replyToMessageId: message.MessageId, replyMarkup: stopInline);

                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("no"))
                    {
                        var inline = new InlineKeyboardMarkup(new[]
                              {
                            new[]
                            {
                                 InlineKeyboardButton.WithCallbackData("Ролл 🎲"),
                                 InlineKeyboardButton.WithCallbackData("Остановить розыгрыш ⛔️")
                            }
                        });

                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"Начало розыграша в чате *{message.Chat.Title}*", replyToMessageId: message.MessageId, parseMode: ParseMode.Markdown, replyMarkup: inline);
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("more"))
                    {
                        await Program.botClient.SendTextMessageAsync(Chat_Id, "Для начала нового розыграша, завершите старый!", replyToMessageId: message.MessageId, replyMarkup: stopInline);
                    }
                }
            }
        }
    }
}
