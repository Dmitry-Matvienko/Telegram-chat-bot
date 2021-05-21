using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram_bot.OrderPhoneNumber
{
    class Order
    {
        public static async void OrderPhone(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                string Path = @"id.txt";
                string id = System.IO.File.ReadAllText(Path);
                var SmsActivate = new System.Net.WebClient();

                if (e.CallbackQuery.Data.Equals("Баланс"))
                {
                    string Balance = SmsActivate.DownloadString("https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=getBalance"); // TODO: Поставить Ваш API-key

                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, Balance.Remove(0, 15) + " p. - доступный баланс");
                }

                else if (e.CallbackQuery.Data.Equals("Заказать номер"))
                {
                    string Order = SmsActivate.DownloadString("https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=getNumber&service=tg");

                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"Id заказа:Номер телефон \n{Order}\nВведи в чат Id своего заказа");
                }

                else if (e.CallbackQuery.Data.Equals("Кол-во доступных номеров"))
                {
                    var AccessNubmer = SmsActivate.DownloadString("https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=getNumbersStatus&country=0&service=tg");

                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"Количество доступных номеров по России: {AccessNubmer}");
                }

                else if (e.CallbackQuery.Data.Equals("Статус заказа"))
                {
                    var Status = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=getStatus&id={id}");

                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"Статус активации: {Status}");
                }

                else if (e.CallbackQuery.Data.Equals("Цены"))
                {
                    var Cost = SmsActivate.DownloadString("https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=getPrices&service=tg&country=0");

                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"Актуальна цена на оренду номера телеграм: {Cost}");
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

                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Выбери статус номера ", replyMarkup: InlineStatus);
                }

                else if (e.CallbackQuery.Data == ("Cообщить о готовности номера"))
                {
                    var ReadyActivate = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=setStatus&status=1&id={id}");
                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"Статус активации изменен: {ReadyActivate}");
                }
                else if (e.CallbackQuery.Data == ("Запросить еще один код"))
                {
                    var NewCode = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=setStatus&status=3&id={id}");
                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"Новый код: {NewCode}");
                }
                else if (e.CallbackQuery.Data == ("Завершить активацию"))
                {
                    var EndActivate = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=setStatus&status=6&id={id}");
                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"Статус активации: {EndActivate}");
                }
                else if (e.CallbackQuery.Data == ("Отменить активацию"))
                {
                    var CancelOrder = SmsActivate.DownloadString($"https://sms-activate.ru/stubs/handler_api.php?api_key=$api_key&action=setStatus&status=8&id={id}");
                    await Program.botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, $"Статус активации: {CancelOrder}");
                }
            }
            catch { }
        }
    }
}
