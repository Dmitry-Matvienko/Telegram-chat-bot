using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using System.Text;
using System.Linq;
using System.Data;

namespace Telegram_bot.Crocodile
{
    class CrocoChatIDTable
    {
        public int id { get; set; }
        public long ChatIds { get; set; }
        public string word { get; set; }
        public int UserId { get; set; }
    }
    class CrocodileGame
    {
        public static async void CrocoButtons(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                if (e.CallbackQuery.Data.Equals("Слово"))
                {
                    using (var SaveWord = new DataBaseBot())
                    {
                        var ShowSaveWord = new CrocoChatIDTable()
                        {
                            ChatIds = e.CallbackQuery.Message.Chat.Id,
                            UserId = e.CallbackQuery.From.Id
                        };

                        var CurrentWord = SaveWord.CrocoChatIDTables.Single(x => x.ChatIds == ShowSaveWord.ChatIds && x.UserId == ShowSaveWord.UserId);
                        await Program.botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, CurrentWord.word, showAlert: true); // Показываем слово из столбца в бд
                    }
                }

                if (e.CallbackQuery.Data.Equals("Поменять слово"))
                {
                    using (var ChangeWord = new DataBaseBot())
                    {
                        var ChangeNextWord = new CrocoChatIDTable()
                        {
                            ChatIds = e.CallbackQuery.Message.Chat.Id,
                            UserId = e.CallbackQuery.From.Id,
                            word = ReturnWord()
                        };

                        var CurrentWord = ChangeWord.CrocoChatIDTables.Single(x => x.ChatIds == ChangeNextWord.ChatIds && x.UserId == ChangeNextWord.UserId);
                        ChangeWord.CrocoChatIDTables.Remove(CurrentWord);
                        ChangeWord.SaveChanges();
                        ChangeWord.CrocoChatIDTables.Add(ChangeNextWord);
                        ChangeWord.SaveChanges();
                        await Program.botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, ChangeNextWord.word, showAlert: true);
                    }
                }

                if (e.CallbackQuery.Data.Equals("Завершить игру"))
                {
                    string Name = $"{e.CallbackQuery.From.FirstName}"; // Имя того кто нажал на кнопку
                    using (var ChangeWord = new DataBaseBot())
                    {
                        var StopGame = new CrocoChatIDTable()
                        {
                            ChatIds = e.CallbackQuery.Message.Chat.Id,
                            UserId = e.CallbackQuery.From.Id
                        };

                        var CurrentChatId = ChangeWord.CrocoChatIDTables.Single(x => x.ChatIds == StopGame.ChatIds && x.UserId == StopGame.UserId);
                        await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id,
                                                             e.CallbackQuery.Message.MessageId,
                                                             $"[{Name}](tg://user?id={e.CallbackQuery.From.Id}) завершил игру", parseMode: ParseMode.Markdown); // Меняем текст сообщения
                        ChangeWord.CrocoChatIDTables.Remove(CurrentChatId);
                        ChangeWord.SaveChanges();
                    }
                }
            }
            catch (InvalidOperationException)
            { await Program.botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ты не ведущий!", showAlert: true); }
           
            try
            {
                if (e.CallbackQuery.Data.Equals("Завершить прошлую игру"))
                {
                    using (var DeleteLastGame = new DataBaseBot())
                    {
                        var LastGame = new CrocoChatIDTable()
                        {
                            ChatIds = e.CallbackQuery.Message.Chat.Id
                        };

                        var DeleteGame = DeleteLastGame.CrocoChatIDTables.Single(x => x.ChatIds == LastGame.ChatIds);
                        DeleteLastGame.CrocoChatIDTables.Remove(DeleteGame);
                        DeleteLastGame.SaveChanges();
                        await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"[{e.CallbackQuery.From.FirstName}](tg://user?id={e.CallbackQuery.From.Id}), игра успешно завершена!", parseMode: ParseMode.Markdown);
                        await Program.botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, messageId: e.CallbackQuery.Message.MessageId);
                    }
                }
            }
            catch { }

            if (e.CallbackQuery.Data.Equals("Хочу быть ведущим"))
            {
                try
                {
                    using (var RecordChatId = new DataBaseBot())
                    {
                        var FirstRecordData = new CrocoChatIDTable()
                        {
                            ChatIds = e.CallbackQuery.Message.Chat.Id
                        };
                        var CheckCurrentGame = RecordChatId.CrocoChatIDTables.Single(x => x.ChatIds == FirstRecordData.ChatIds); // проверка ,есть ли чат в бд

                    }
                    await Program.botClient.EditMessageReplyMarkupAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, null); // Удаляем inline клавиатуру
                    await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, $"Игра уже начата!", parseMode: ParseMode.Markdown);
                }
                catch (MessageIsNotModifiedException) { }
                catch (Exception)
                {
                    using (var NewGame = new DataBaseBot())
                    {
                        var Word_for_new_game = new CrocoChatIDTable()
                        {
                            ChatIds = e.CallbackQuery.Message.Chat.Id,
                            UserId = e.CallbackQuery.From.Id,
                            word = ReturnWord()
                        };

                        NewGame.CrocoChatIDTables.Add(Word_for_new_game);
                        NewGame.SaveChanges();

                    }
                    var InlineAfter = new InlineKeyboardMarkup(new[]
                {
                     new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Слово")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Поменять слово")
                    },
                    new[]
                    {
                       InlineKeyboardButton.WithCallbackData("Завершить игру")
                    }
                }); // создание трех кнопок(три строки)
                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"[{e.CallbackQuery.From.FirstName}](tg://user?id={e.CallbackQuery.From.Id}), твой черед объяснять слово", parseMode: ParseMode.Markdown, replyMarkup: InlineAfter);
                    await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, "Игра начата!");
                }
            }
        }

        public static async void StartGame(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long Chat_Id = e.Message.Chat.Id;

            if (message.Text == "/terpilacroco@terpilla_bot" && Chat_Id != message.From.Id
             || message.Text == "/terpilacroco" && Chat_Id != message.From.Id) // Игра "крокодил"
            {
                try
                {
                    using (var RecordChatId = new DataBaseBot())
                    {
                        var FirstRecordData = new CrocoChatIDTable()
                        {
                            ChatIds = Chat_Id
                        };
                        var CheckCurrentGame = RecordChatId.CrocoChatIDTables.Single(x => x.ChatIds == FirstRecordData.ChatIds); // проверка ,есть ли чат в бд
                    }

                    var inline = new InlineKeyboardMarkup(new[]
                           {
                            new[]
                                 {
                                 InlineKeyboardButton.WithCallbackData("Завершить прошлую игру")
                                 }
                            });

                    await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}), " +
                        $"для того чтобы начать новую игру, необходимо закончить старую!", parseMode: ParseMode.Markdown, replyMarkup: inline);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("no")) // Исключеение, если в таблице отсутствует чат(начинаем игру)
                {
                    using (var RecordChatId = new DataBaseBot())
                    {
                        var FirstRecordData = new CrocoChatIDTable()
                        {
                            ChatIds = Chat_Id,
                            word = ReturnWord(),
                            UserId = message.From.Id
                        }; // записываем игру в бд

                        RecordChatId.CrocoChatIDTables.Add(FirstRecordData);
                        RecordChatId.SaveChanges();

                        var inline = new InlineKeyboardMarkup(new[]
                            {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Слово")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Поменять слово")
                    },
                    new[]
                    {
                       InlineKeyboardButton.WithCallbackData("Завершить игру")
                    }
                        }); // создание двух кнопок(две строки)
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}), твой черед объяснять слово\n", parseMode: ParseMode.Markdown, replyMarkup: inline);
                    }

                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("more")) // Исключение если в таблице больше одной записи чата
                {
                    using (var DeleteAllSameChats = new DataBaseBot())
                    {
                        var crocoChatID = new CrocoChatIDTable()
                        {
                            ChatIds = Chat_Id
                        };
                        DeleteAllSameChats.CrocoChatIDTables.RemoveRange(DeleteAllSameChats.CrocoChatIDTables.Where(x => x.ChatIds == crocoChatID.ChatIds));
                        DeleteAllSameChats.SaveChanges();
                    }
                    await Program.botClient.SendTextMessageAsync(Chat_Id, $"Произошел сбой, причина:\n*Одновременное нажатие начала игры нескольких пользователей*\nПроблема устранена, начните игру заново", parseMode: ParseMode.Markdown);
                }
                catch (Exception) {}
            }

            try
            {
                using (var RightWord = new DataBaseBot())
                {
                    var Right_Word = new CrocoChatIDTable()
                    {
                        ChatIds = Chat_Id,
                        UserId = message.From.Id
                    };

                    if (message.Type == MessageType.Text)
                    {
                        var CurrentChatId = RightWord.CrocoChatIDTables.Single(x => x.ChatIds == Right_Word.ChatIds);

                        if (CurrentChatId.UserId == message.From.Id && message.Text.Contains($"{CurrentChatId.word}", StringComparison.CurrentCultureIgnoreCase))
                        {
                            await Program.botClient.SendTextMessageAsync(chatId: e.Message.Chat, "Ну и зачем?", replyToMessageId: message.MessageId);
                        }

                        else if (CurrentChatId.UserId != message.From.Id && message.Text.Contains($"{CurrentChatId.word}", StringComparison.CurrentCultureIgnoreCase))
                        {
                            await Program.botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"[{e.Message.From.FirstName}](tg://user?id={e.Message.From.Id}), ты выиграл\n" +
                            $"Правильное слово - {CurrentChatId.word}", parseMode: ParseMode.Markdown, replyToMessageId: e.Message.MessageId);

                            var NewInline = new InlineKeyboardMarkup(new[]
                            {
                               new[]
                              {
                                   InlineKeyboardButton.WithCallbackData("Хочу быть ведущим")
                              },
                            });
                            await Program.botClient.SendTextMessageAsync(chatId: e.Message.Chat, "Кто будет играть?", replyMarkup: NewInline);
                            RightWord.CrocoChatIDTables.Remove(CurrentChatId);
                            RightWord.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception) {}
        }

        public static string ReturnWord()
        {
            string list_words = @"words.txt";
            var random = new Random();
            string[] random_word = System.IO.File.ReadAllLines(list_words, Encoding.UTF8);
            string out_word = random_word[random.Next(random_word.Length)];
            return out_word; // Возвращаем слово для игры
        }
    }

}
