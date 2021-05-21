using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ApiAiSDK;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Telegram_bot
{
    class Program
    {
        public static ITelegramBotClient botClient;
        private static ApiAi apiAi_for_pm;   // объявление переменной для общения в ЛС с ботом с помощью DialogFlow
        private static ApiAi apiAi_for_chat; // объявление переменной для общения в чате с ботом с помощью DialogFlow

        static async Task Main(string[] args)
        {
            botClient = new TelegramBotClient("") { Timeout = TimeSpan.FromSeconds(10) }; // вводим токен бота
            AIConfiguration configAi_for_pm = new AIConfiguration("", SupportedLanguage.Russian);     // ключ из DialogFlow (для общения в ЛС)
            AIConfiguration configAi_for_chat = new AIConfiguration("", SupportedLanguage.Russian);   // ключ из DialogFlow (для общения в чате)

            apiAi_for_pm = new ApiAi(configAi_for_pm);
            apiAi_for_chat = new ApiAi(configAi_for_chat);

            var Bot = await botClient.GetMeAsync();
            Console.WriteLine($"Имя бота: {Bot.FirstName}; Id бота {Bot.Id} ");
            
            botClient.OnMessage += ChatAction.ChatActions.WelcomesAndChanges;
            botClient.OnMessage += ForAdmins.GroupAdministrations.UserControl;
            botClient.OnCallbackQuery += OrderPhoneNumber.Order.OrderPhone;
            botClient.OnMessage += Reports.ReportForAdmins.SendReport;
            botClient.OnMessage += RestrictMedia.Restrict.RestrictsMedia;
            botClient.OnMessage += Weather.TakeWeather.FindWeather;

            botClient.OnCallbackQuery += RollGame.RollGames.RollButtons;
            botClient.OnMessage += RollGame.RollGames.StartEvent;

            botClient.OnCallbackQuery += Crocodile.CrocodileGame.CrocoButtons;
            botClient.OnMessage += Crocodile.CrocodileGame.StartGame;

            botClient.OnMessage += MessagesAndRating.Messages.TextMessages;
            botClient.OnMessage += MessagesAndRating.Rate.Rating;


            botClient.OnMessage += BotOnMessageReceived;
            botClient.OnCallbackQuery += BotClient_OnCallbackQuery;

            botClient.StartReceiving();
            Console.ReadKey();
            botClient.StopReceiving();
        }

        private static async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            long ChatId   = e.CallbackQuery.Message.Chat.Id;     // Id чата

            try 
            {
                if (e.CallbackQuery.Data == ("Для админов"))
                {
                    using (var AdminFile = new StreamReader(@"admins.txt"))
                    {
                        await botClient.SendTextMessageAsync(ChatId, $"{AdminFile.ReadToEnd()}");
                    }
                }

                else if (e.CallbackQuery.Data == ("Для участников"))
                {
                    using (var UserFile = new StreamReader(@"users.txt"))
                    {
                        await botClient.SendTextMessageAsync(ChatId, $"{UserFile.ReadToEnd()}");
                    }
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

            var photo = new ChoosePhoto();

            try
            {
                if (MessageText == "/start" && ChatId == UserId)
                {
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat,"Приветствую тебя. Хочешь поговорить со мной? :)").ConfigureAwait(false);
                }
                if (MessageText == "Заказ" && ChatId == UserId) // Взаимодействие с API sms-activate.ru
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

                if (int.TryParse(MessageText, out int IdOrder) && MessageText.Length >= 9 && ChatId == UserId)
                {
                    string Path_to_id = @"id.txt"; // файл c номером заказа
                    using (StreamWriter Id = new StreamWriter(Path_to_id, false, Encoding.UTF8))
                    {
                        Id.WriteLine(IdOrder);
                    }
                    await botClient.SendTextMessageAsync(ChatId, $"Заказ с Id {IdOrder} запомнил");
                }


                if (ChatId == 1234567890 && MessageText.Substring(0, 9) == "Добавить:") // TODO: поставить свой id
                {
                    // Добавляем новые слова в бд для таблицы "секретных слов"
                    var DeleteFirstWord = MessageText.Remove(0, 9);
                    String[] DropString = DeleteFirstWord.Split(new char[] { ' ', ',', '|' }, StringSplitOptions.RemoveEmptyEntries); // Удалить все ненужные элементы во входной строке

                    using (var AddsecretWords = new DataBaseBot())
                    {

                        foreach (var ScrollWords in DropString)
                        {
                            var secret = new MessagesAndRating.SecretWordTable()
                            {
                                words = ScrollWords
                            };
                            AddsecretWords.SecretWordTables.Add(secret);
                            AddsecretWords.SaveChanges();
                            await botClient.SendTextMessageAsync(ChatId, ScrollWords);
                        }
                    }
                }

            }
            catch { }

            try
            {
                var admin = botClient.GetChatMemberAsync(ChatId, userId: e.Message.ReplyToMessage.From.Id).Result; // инфорамация про участника, которому был реплай

                if (MessageText == "Права" && ReplyMessage != null && admin.Status == ChatMemberStatus.Administrator) // Проверять права администраторов
                {

                    await botClient.SendTextMessageAsync(ChatId, $"*{admin.Status}* [{e.Message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) может:" +
                        $"\nУдалять сообщения: *{admin.CanDeleteMessages}*" +
                        $"\nЗакреплять сообщения: *{admin.CanPinMessages}*" +
                        $"\nДобавлять администрацию: *{admin.CanPromoteMembers}*" +
                        $"\nОграничивать/банить пользователей: *{admin.CanRestrictMembers}*" +
                        $"\nИзменять описание группы: *{admin.CanChangeInfo}*" +
                        $"\nПриглашать людей: *{admin.CanInviteUsers}*", parseMode: ParseMode.Markdown);
                }

                else if (MessageText == "Фото")
                {
                    await botClient.SendPhotoAsync(ChatId, photo.SavePhoto(), replyToMessageId: MessageId);   // Отправка случайного фото
                }

            }
            catch { }

            try {

                if (MessageText == "Стат" || MessageText == "/mystat@terpilla_bot" || MessageText == "/mystat" && ChatId != UserId)
                {
                    using (var context = new DataBaseBot())
                    {
                        var group = new MessagesAndRating.RatingTable()
                        {
                            FirstName = e.Message.From.FirstName,
                            UserId = e.Message.From.Id
                        };

                        var count = new MessagesAndRating.CountMessageTable()
                        {
                            FirstName = e.Message.From.FirstName,
                            UserId = e.Message.From.Id
                        };

                        try
                        {
                            var counter = context.CountMessageTables.Single(x => x.UserId == count.UserId);
                            var s = context.RatingTables.Single(x => x.UserId == group.UserId);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat,
                            text: $"[{FirstName}](tg://user?id={UserId})\nID чата: *{ChatId}*\nВсего сообщений в чате : " +
                            $" *{e.Message.MessageId}*\nТвой рейтинг: *{s.rate}* 🆙\nТвои сообщения по всем чатам: *{counter.Counter}* ✉️\n" +
                            $"Твое звание: *{counter.rank}*\nДата: *{e.Message.Date}* (UTC + 3 часа)"
                            , parseMode: ParseMode.Markdown).ConfigureAwait(false);
                        }
                        catch (InvalidOperationException)
                        { await botClient.SendTextMessageAsync(chatId: e.Message.Chat, $"[{FirstName}](tg://user?id={UserId}), для показа статистики у тебя должен быть рейтинг", parseMode: ParseMode.Markdown); }


                    }
                }

                if (MessageText == "/help@terpilla_bot" || MessageText == "/help") // Инструкция к боту
                {
                    var choose = new InlineKeyboardMarkup(new[]
                    {
                    new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Для админов")
                        },
                     new[]{
                            InlineKeyboardButton.WithCallbackData("Для участников")
                          }
                    });

                    await botClient.SendTextMessageAsync(ChatId, "Инструкция:", replyMarkup: choose);
                }

            } catch { }
            
            try
            {
                if (e.Message.Type == MessageType.Text)                    //проверка триггеров для общения в ЛС
                {
                    var responsed = apiAi_for_pm.TextRequest(MessageText); // Сообщение которое будет отправленно в DialogFlow
                    string answer_pm = responsed.Result.Fulfillment.Speech;// Получаем ответ от DialogFlow
                    await botClient.SendTextMessageAsync(chatId: e.Message.From.Id, answer_pm, replyToMessageId: MessageId);
                }

                if (e.Message.Type == MessageType.Text) //проверка триггеров для общения в чате
                {
                    var responsed_1 = apiAi_for_chat.TextRequest(MessageText);
                    string answer_chat = responsed_1.Result.Fulfillment.Speech;
                    await botClient.SendTextMessageAsync(chatId: ChatId, answer_chat, replyToMessageId: MessageId);
                }
            }
            catch { }

            string name = $"{FirstName} ({e.Message.From.Username})";
            Console.WriteLine($"{name} написал : {MessageText}");
        }
    }
}
