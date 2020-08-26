using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ApiAiSDK;
using System.IO;
using System.Text;
using System.Linq;

namespace Telegram_chat_bot
{
    class Program
    {
        private static ITelegramBotClient botClient;
        private static ApiAi apiAi_for_pm;   // объявление переменной для общения в ЛС с ботом с помощью DialogFlow
        private static ApiAi apiAi_for_chat; // объявление переменной для общения в чате с ботом с помощью DialogFlow

        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("") { Timeout = TimeSpan.FromSeconds(10) }; // вводим токен бота
            AIConfiguration configAi_for_pm = new AIConfiguration("", SupportedLanguage.Russian);     // ключ из DialogFlow (для общения в ЛС)
            AIConfiguration configAi_for_chat = new AIConfiguration("", SupportedLanguage.Russian);   // ключ из DialogFlow (для общения в чате)

            apiAi_for_pm = new ApiAi(configAi_for_pm);
            apiAi_for_chat = new ApiAi(configAi_for_chat);

            var Bot = botClient.GetMeAsync().Result;
            Console.WriteLine($"Имя бота: {Bot.FirstName}; Id бота {Bot.Id} ");

            botClient.OnMessage += BotOnMessageReceived;
            botClient.OnCallbackQuery += BotClient_OnCallbackQuery;

            botClient.StartReceiving();
            Console.ReadKey();
            botClient.StopReceiving();
        }

        private static async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {

            string Name   = $"{e.CallbackQuery.From.FirstName}"; // Имя того кто нажал на кнопку
            long ChatId   = e.CallbackQuery.Message.Chat.Id;     // Id чата
            var MessageId = e.CallbackQuery.Message.MessageId;   // Id сообщения
            var UserId    = e.CallbackQuery.From.Id;             // Id участника чата

            var word = new ListWords();

            string Path_Id = @"C:.txt"; // Путь к записанному Id для игры
            int Id_Pressing = e.CallbackQuery.From.Id;                 // Id участника, который нажмет "Хочу быть ведущим"
            string Check_Id = System.IO.File.ReadAllText(Path_Id);     // Чтение записанного Id
            int Right_Id = Convert.ToInt32(Check_Id);

            string WritePath = @"D:.txt";                  // Путь к записанному Id для заказа номера
            string Id = System.IO.File.ReadAllText(WritePath);

            var SmsActivate = new System.Net.WebClient();

            try
            {
                if (e.CallbackQuery.Data.Equals("Первое слово") && UserId == Right_Id)
                {
                    await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, word.ReturnWord(), showAlert: true); // Возвращаем случайное слово из .txt

                    var InlineAfter_StartWord = new InlineKeyboardMarkup(new[]
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
                    }); // Создаем три новые Inline кнопки

                    await botClient.EditMessageReplyMarkupAsync(ChatId, MessageId, replyMarkup: InlineAfter_StartWord);
                }

                else if (e.CallbackQuery.Data.Equals("Первое слово") && UserId != Right_Id)
                {
                    await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ты не ведущий!", showAlert: true);
                }
            }
            catch { }

            try
            {
                if (e.CallbackQuery.Data.Equals("Завершить игру") && UserId == Right_Id)
                {
                    await botClient.EditMessageReplyMarkupAsync(ChatId, MessageId, null); // Удаляем inline клавиатуру
                    await botClient.EditMessageTextAsync(ChatId, MessageId,
                    $"[{Name}](tg://user?id={UserId}) завершил игру", ParseMode.Markdown); // Меняем текст сообщения
                }

                else if (e.CallbackQuery.Data.Equals("Завершить игру") && UserId != Right_Id)
                {
                    await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ты не ведущий!", showAlert: true);
                }
            }
            catch { }

            try
            {
                if (e.CallbackQuery.Data.Equals("Слово") && UserId == Right_Id)
                {
                    await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, word.SaveReturnWord(), showAlert: true); // Возвращаем сохраненное слово 
                }

                else if (e.CallbackQuery.Data.Equals("Слово") && UserId != Right_Id)
                {
                    await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ты не ведущий!", showAlert: true);
                }
            }
            catch { }

            try
            {
                if (e.CallbackQuery.Data.Equals("Поменять слово") && UserId == Right_Id)
                {
                    await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, word.ReturnWord(), showAlert: true);
                }

                else if (e.CallbackQuery.Data.Equals("Поменять слово") && UserId != Right_Id)
                {
                    await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ты не ведущий!", showAlert: true);
                }
            }
            catch { }

            try
            {
                if (e.CallbackQuery.Data.Equals("Хочу быть ведущим"))
                {
                    using (StreamWriter Record = new StreamWriter(Path_Id, false, System.Text.Encoding.UTF8))
                    {
                        Record.WriteLine(Id_Pressing); // Записываем Id того кто нажал в .txt файл для игры
                    }

                    var InlineAfter = new InlineKeyboardMarkup(new[]
                    {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Первое слово")
                    },
                    new[]
                    {
                       InlineKeyboardButton.WithCallbackData("Завершить игру")
                    }
                    });
                    await botClient.SendTextMessageAsync(ChatId,
                    $"[{Name}](tg://user?id={UserId}), твой черед объяснять слово", parseMode: ParseMode.Markdown, replyMarkup: InlineAfter);
                    await botClient.EditMessageReplyMarkupAsync(ChatId, MessageId, null);
                    await botClient.EditMessageTextAsync(ChatId, MessageId, "Игра начата!"); // Начало новой игры
                }
            }
            catch { }

            try
            {
                if (e.CallbackQuery.Data.Equals("Баланс"))
                {
                    string Balance = SmsActivate.DownloadString("https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=getBalance");

                    await botClient.SendTextMessageAsync(ChatId, Balance.Remove(0, 15) + " p. - доступный баланс");
                }

                else if (e.CallbackQuery.Data.Equals("Заказать номер"))
                {
                    string Order = SmsActivate.DownloadString("https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=getNumber&service=tg");

                    await botClient.SendTextMessageAsync(ChatId, $"Id заказа:Номер телефон \n{Order}\nВведи в чат Id своего заказа");
                }

                else if (e.CallbackQuery.Data.Equals("Кол-во доступных номеров"))
                {
                    var AccessNubmer = SmsActivate.DownloadString("https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=getNumbersStatus&country=0&service=tg");

                    await botClient.SendTextMessageAsync(ChatId, $"Количество доступных номеров по России: {AccessNubmer}");
                }

                else if (e.CallbackQuery.Data.Equals("Статус заказа"))
                {
                    var Status = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=getStatus&id={Id}");

                    await botClient.SendTextMessageAsync(ChatId, $"Статус активации: {Status}");
                }

                else if (e.CallbackQuery.Data.Equals("Цены"))
                {
                    var Cost = SmsActivate.DownloadString("https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=getPrices&service=tg&country=0");

                    await botClient.SendTextMessageAsync(ChatId, $"Актуальна цена на оренду номера телеграм: {Cost}");
                }

                else if (e.CallbackQuery.Data.Equals("Изменить статус"))
                {
                    var InlineStatus = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Cообщить о готовности номера")
                        },

                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Запросить еще один код")
                        },

                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Завершить активацию")
                        },

                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Отменить активацию")
                        }
                    }); // Новая Inline клавиатура для изменения статуса заказа

                    await botClient.SendTextMessageAsync(ChatId, "Выбери статус номера ", replyMarkup: InlineStatus);
                }
            }
            catch { }

            try
            {
                if (e.CallbackQuery.Data.Equals("Cообщить о готовности номера"))
                {
                    var ReadyActivate = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=setStatus&status=1&id={Id}");

                    await botClient.SendTextMessageAsync(ChatId, $"Статус активации изменен: {ReadyActivate}");
                }

                else if (e.CallbackQuery.Data.Equals("Запросить еще один код"))
                {
                    var NewCode = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=setStatus&status=3&id={Id}");

                    await botClient.SendTextMessageAsync(ChatId, $"Новый код: {NewCode}");
                }

                else if (e.CallbackQuery.Data.Equals("Завершить активацию"))
                {
                    var EndActivate = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=setStatus&status=6&id={Id}");

                    await botClient.SendTextMessageAsync(ChatId, $"Статус активации: {EndActivate}");
                }

                else if (e.CallbackQuery.Data.Equals("Отменить активацию"))
                {
                    var CancelOrder = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=59d39b6633A111d1f02b4561c475A02d&action=setStatus&status=8&id={Id}");

                    await botClient.SendTextMessageAsync(ChatId, $"Статус активации: {CancelOrder}");
                }
            }
            catch { }

        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message == null)
            {
                return;
            }

            string MessageText = e.Message.Text;           // Текстовое сообщение
            long ChatId        = e.Message.Chat.Id;        // Id чата
            int UserId         = e.Message.From.Id;        // Id участника чата
            var ReplyMessage   = e.Message.ReplyToMessage; // Сообщение которому ответили
            int MessageId      = e.Message.MessageId;      // Id сообщения
            string FirstName   = e.Message.From.FirstName; // Ник участника чата

            ChatPermissions chatPermissions = new ChatPermissions(); // Переменная для действия "Мут"

            ChatPermissions Permissions_during_restrict = new ChatPermissions
            {
                CanInviteUsers       = false,
                CanSendMediaMessages = false,
                CanPinMessages       = false,
                CanSendMessages      = true,
                CanSendOtherMessages = true
            };
            ChatPermissions Permissions_after_restrict = new ChatPermissions
            {
                CanSendMediaMessages = true,
                CanInviteUsers       = true,
                CanSendOtherMessages = true,
                CanSendMessages      = true
            };

            Stream VideoFile = System.IO.File.OpenRead(@"C:.mp4");

            string Path_Id = @"C:\record.txt"; // .txt файл для записи Id для игры(файл должен быть с кодировкой UTF-8)
            int Line_Id = e.Message.From.Id;                           // Id того кто начнет игру
            string Check_Id = System.IO.File.ReadAllText(Path_Id);     // Просматриваем Id который записан в файле
            int Right_Id = Convert.ToInt32(Check_Id);
            ListWords RightWord = new ListWords();                     // Правильное слово из списка для игры

            var photo = new ChoosePhoto();

            if (e.Message.Type == MessageType.Photo && ChatId == { Ваш Id })
            {
                await botClient.SendPhotoAsync(chatId: { Id Чата}, photo: e.Message.Photo[e.Message.Photo.Count() - 1].FileId, caption: $"{e.Message.Caption}"); // Отправка файла в конкретный чат
            }

            if (MessageText == "Заказ" && ChatId == UserId)
            {
                var InlineOrder = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Баланс"),
                        InlineKeyboardButton.WithCallbackData("Заказать номер")
                    },

                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Кол-во доступных номеров")
                    },
                    new[]

                    {
                        InlineKeyboardButton.WithCallbackData("Статус заказа"),
                        InlineKeyboardButton.WithCallbackData("Цены")
                    },

                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Изменить статус")
                    },

                }); // Inline клавиатура для выполнения заказа номера

                await botClient.SendTextMessageAsync(ChatId, "Выбери одну из следующих операций:", replyMarkup: InlineOrder);
            }

            if (int.TryParse(MessageText, out int IdOrder) && MessageText.Length >= 9 &&
                ChatId == UserId)
            {
                string Path_to_id = @"D:\.txt"; // файл c номером заказа
                using (StreamWriter Id = new StreamWriter(Path_to_id, false, Encoding.UTF8))
                {
                    Id.WriteLine(IdOrder);
                }
                await botClient.SendTextMessageAsync(ChatId, $"Заказ с Id {IdOrder} запомнил");
            }

            try
            {
                if (e.Message.Type == MessageType.ChatMembersAdded)
                {
                    await botClient.SendTextMessageAsync(ChatId,
                    $"[{e.Message.NewChatMembers[0].FirstName}](tg://user?id={e.Message.NewChatMembers[0].Id}), приветствуем тебя в {e.Message.Chat.Title} чате!" +
                    $"\n❗️Заметил(а) нарушение?\nНапиши в чате - !админ❗️", parseMode: ParseMode.Markdown, replyToMessageId: MessageId);

                }

                else if (e.Message.Type == MessageType.ChatMemberLeft)
                {
                    await botClient.SendTextMessageAsync(ChatId,
                    $" Еще один - [{e.Message.LeftChatMember.FirstName}](tg://user?id={e.Message.LeftChatMember.Id}) слился", parseMode: ParseMode.Markdown);
                }
            }
            catch { }

            try
            {
                var admin = botClient.GetChatMemberAsync(ChatId, UserId).Result; // Информация об одном из участников чата

                if (MessageText == "Бан" && admin.CanDeleteMessages == true && ReplyMessage != null ||
                    MessageText == "Бан" && admin.Status == ChatMemberStatus.Creator && ReplyMessage != null)
                {
                    await botClient.SendTextMessageAsync(ChatId, $"[{FirstName}](tg://user?id={UserId}) забанил участника [{ReplyMessage.From.FirstName}](tg://user?id={ReplyMessage.From.Id})", parseMode: ParseMode.Markdown);
                    await botClient.KickChatMemberAsync(ChatId, userId: ReplyMessage.From.Id); // бан пользователЮ, которому был реплай
                }

                else if (MessageText == "Бан" && admin.Status != ChatMemberStatus.Administrator && ReplyMessage != null)
                {
                    await botClient.SendTextMessageAsync(ChatId, $"[{FirstName}](tg://user?id={UserId}), ах ты жопа хитрая\nНизя так :)", replyToMessageId: MessageId, parseMode: ParseMode.Markdown);
                }


                if (MessageText == "Мут" && admin.CanDeleteMessages == true && ReplyMessage != null ||
                    MessageText == "Мут" && admin.Status == ChatMemberStatus.Creator && ReplyMessage != null)
                {
                    await botClient.SendTextMessageAsync(ChatId, $"[{FirstName}](tg://user?id={UserId}) " +
                    $"дал мут участнику [{ReplyMessage.From.FirstName}](tg://user?id={ReplyMessage.From.Id}) на 10 минут", parseMode: ParseMode.Markdown);
                    await botClient.RestrictChatMemberAsync(ChatId, userId: ReplyMessage.From.Id,
                    permissions: chatPermissions, untilDate: DateTime.UtcNow.AddMinutes(10)); // Мут пользователю на 10 минут которому был реплай 
                }

                if (MessageText == "Кик" && admin.CanDeleteMessages == true && ReplyMessage != null ||
                    MessageText == "Кик" && admin.Status == ChatMemberStatus.Creator && ReplyMessage != null)
                {
                    await botClient.SendTextMessageAsync(ChatId, $"[{FirstName}](tg://user?id={UserId}) " +
                    $"кикнул участника [{e.Message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id})", parseMode: ParseMode.Markdown);
                    await botClient.KickChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id); // Бан пользователю, которому был реплай
                    await botClient.UnbanChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id);// Разбан того же пользователя
                }

                if (MessageText == "Опрос" && admin.CanPromoteMembers == true ||
                    MessageText == "Опрос" && admin.Status == ChatMemberStatus.Creator)
                {
                    await botClient.SendPollAsync(ChatId, question: "Как проходит ваш день?",
                            options: new[]
                            {
                            "На работе",
                            "Дома отдыхаю",
                            "Пoгибаю от скуки"
                            });
                    await botClient.PinChatMessageAsync(ChatId, messageId: MessageId);
                }

                if (e.Message.Type == MessageType.Dice)
                {
                    await botClient.SendDiceAsync(ChatId, emoji: Emoji.Basketball, replyToMessageId: MessageId);
                }
                #region
                if (UserId == 670333173 && ReplyMessage != null ||
                    admin.Status == ChatMemberStatus.Creator && ReplyMessage != null)
                {
                    switch (MessageText) // Управление правами администратора
                    {
                        case "Разрешить удаление":
                            await botClient.SendTextMessageAsync(ChatId, $"[{e.Message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) теперь может удалять сообщения и банить участников", parseMode: ParseMode.Markdown);
                            await botClient.PromoteChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id,
                                   canDeleteMessages: true); break;

                        case "Разрешить мут":
                            await botClient.SendTextMessageAsync(ChatId, $"[{ReplyMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) теперь может давать мут участникам чата", parseMode: ParseMode.Markdown);
                            await botClient.PromoteChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id,
                                   canRestrictMembers: true); break;

                        case "Разрешить закреп":
                            await botClient.SendTextMessageAsync(ChatId, $"[{e.Message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) теперь может закреплять сообщения", parseMode: ParseMode.Markdown);
                            await botClient.PromoteChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id,
                                   canPinMessages: true); break;

                        case "Разрешить инвайт":
                            await botClient.SendTextMessageAsync(ChatId, $"[{e.Message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) теперь может приглашать юзеров", parseMode: ParseMode.Markdown);
                            await botClient.PromoteChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id,
                                   canInviteUsers: true); break;

                        case "Дать админку":
                            await botClient.SendTextMessageAsync(ChatId, $"[{e.Message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) теперь админ", parseMode: ParseMode.Markdown);
                            await botClient.PromoteChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id,
                                   canRestrictMembers: true,
                                   canDeleteMessages: true,
                                   canChangeInfo: true,
                                   canPinMessages: true,
                                   canInviteUsers: true); break;

                        case "Забрать админку":
                            await botClient.SendTextMessageAsync(ChatId, $"У [{e.Message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) забрали админку :(", parseMode: ParseMode.Markdown);
                            await botClient.PromoteChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id,
                                   canRestrictMembers: false,
                                   canDeleteMessages: false,
                                   canChangeInfo: false,
                                   canPinMessages: false,
                                   canInviteUsers: false,
                                   canPostMessages: false); break;
                    }
                }
            }
            catch { }
            #endregion
            try
            {
                if (DateTime.Now.Hour >= 2 && DateTime.Now.Hour < 8)
                {

                    await botClient.SetChatPermissionsAsync(ChatId, permissions: Permissions_during_restrict); // Ограничения в ночное время
                }

                else if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour <= 1)
                {

                    await botClient.SetChatPermissionsAsync(ChatId, permissions: Permissions_after_restrict); // Снятие ограничений
                }

                else if (ChatId == { Ваш Id })
                {
                    await botClient.SendTextMessageAsync(chatId: { Id чата}, MessageText);                // Написать от имени бота в конкретный чат
                }

                else if (MessageText == "Фото")
                {
                    await botClient.SendPhotoAsync(ChatId, photo.SavePhoto(), replyToMessageId: MessageId);   // Отправка случайного фото
                }

                else if (e.Message.Type == MessageType.Photo && ChatId == UserId)
                {
                    await botClient.ForwardMessageAsync(chatId: { Ваш Id }, fromChatId: UserId, e.Message.MessageId);    // Если пользователь отправил фото в лс боту, бот пересывает сообщение в конкретный чат
                }
            }
            catch { }

            try
            {
                if (MessageText == "!админ" && ReplyMessage != null)    // Отправка жалобы на сообщение админу в лс
                {
                    await botClient.SendTextMessageAsync(chatId: 670333173, $"[{FirstName}](tg://user?id={UserId})" +
                        $" пожаловался на сообщение: *''{e.Message.ReplyToMessage.Text}''*\n" +
                        $" участника: [{e.Message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id})",
                        parseMode: ParseMode.Markdown);
                }
            }
            catch { }

            if (MessageText == "/") // Игра "крокодил"
            {
                using (StreamWriter Record_Id = new StreamWriter(Path_Id, false, System.Text.Encoding.UTF8))
                {
                    Record_Id.WriteLine(Line_Id);
                }

                var inline = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Первое слово")
                    },

                    new[]
                    {
                       InlineKeyboardButton.WithCallbackData("Завершить игру")
                    }
                }); // создание трех кнопок(три строки)
                await botClient.SendTextMessageAsync(ChatId, $"[{FirstName}](tg://user?id={UserId}), твой черед объяснять слово", parseMode: ParseMode.Markdown, replyMarkup: inline);
            }

            try
            {
                if (e.Message.Type == MessageType.Text)                    //проверка триггеров для общения в ЛС
                {
                    var responsed = apiAi_for_pm.TextRequest(MessageText); // Сообщение которое будет отправленно в DialogFlow
                    string answer_pm = responsed.Result.Fulfillment.Speech;// Получаем ответ от DialogFlow
                    await botClient.SendTextMessageAsync(chatId: e.Message.From.Id, answer_pm, replyToMessageId: MessageId);
                }
            }
            catch { }

            try
            {
                if (e.Message.Type == MessageType.Text) //проверка триггеров для общения в чате
                {
                    var responsed_1 = apiAi_for_chat.TextRequest(MessageText);
                    string answer_chat = responsed_1.Result.Fulfillment.Speech;
                    await botClient.SendTextMessageAsync(chatId: ChatId, answer_chat, replyToMessageId: MessageId);
                }
            }
            catch { }

            try
            {
                if (e.Message.Type == MessageType.Text &&
                    MessageText.Equals(RightWord.SaveReturnWord(), StringComparison.CurrentCultureIgnoreCase) &&
                    UserId != Right_Id) // Проверка правильности слова для игры
                {
                    await botClient.SendTextMessageAsync(ChatId, text: $"[{FirstName}](tg://user?id={UserId}), ты выиграл\n" +
                    $"Правильное слово - {RightWord.SaveReturnWord()}", parseMode: ParseMode.Markdown, replyToMessageId: e.Message.MessageId);

                    var NewInline = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Хочу быть ведущим")
                        },
                        });
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, "Кто будет играть?", replyMarkup: NewInline);
                }
            }
            catch { }

            try
            {
                switch (MessageText)
                {
                    case "Йожик":
                        await botClient.SendVideoAsync(chatId: e.Message.Chat, video: VideoFile,
                        caption: "Тссс, только никому :)", replyToMessageId: MessageId); break;

                    case "/start":
                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat,
                        text: "Приветствую тебя. Хочешь поговорить со мной? :)").ConfigureAwait(false); break;

                    case "Стат":
                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat,
                        text: $"ID чата: {e.Message.Chat.Id}\nCообщений в чате : " +
                        $" {e.Message.MessageId}\nДата: {DateTime.Now} по МСК").ConfigureAwait(false); break;

                }
            }
            catch { }
            string name = $"{FirstName} ({e.Message.From.Username})";
            Console.WriteLine($"{name} написал : {MessageText}");
        }
    }
}
