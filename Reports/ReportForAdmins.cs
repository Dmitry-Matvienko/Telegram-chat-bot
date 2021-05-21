using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using System.Text;
using System.Linq;
using System.Data;

namespace Telegram_bot.Reports
{
    class AdminTable
    {
        public int id { get; set; }
        public string Firstname { get; set; }
        public long AdminId { get; set; }
        public long Chatid { get; set; }
        public long ChatId2 { get; set; }
    }

    class ReportForAdmins
    {
        public static async void SendReport(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long Chat_Id = e.Message.Chat.Id;
            try
            {
                var admins = await Program.botClient.GetChatAdministratorsAsync(Chat_Id); // получение списка админов
                var admin2 = Program.botClient.GetChatMemberAsync(Chat_Id, userId: message.From.Id).Result; // Проверка участника чата
                
                using (var Add_or_update_admins = new DataBaseBot())
                {
                    if (message.Text == "/update_admins@terpilla_bot" && admin2.Status == ChatMemberStatus.Administrator && Chat_Id != message.From.Id
                     || message.Text == "/update_admins@terpilla_bot" && admin2.Status == ChatMemberStatus.Creator && Chat_Id != message.From.Id
                     || message.Text == "/update_admins" && admin2.Status == ChatMemberStatus.Administrator && Chat_Id != message.From.Id
                     || message.Text == "/update_admins" && admin2.Status == ChatMemberStatus.Creator && Chat_Id != message.From.Id) // Проверяем, обновляем список администраторов 
                    {
                        var sb = new StringBuilder();
                        foreach (var list in admins)
                        {
                            sb.Append($"[{list.User.FirstName}](tg://user?id={list.User.Id})\n"); // Склеиваим весь список админов в одно сообщение
                            var CheckAdmins = new AdminTable()
                            {
                                AdminId = list.User.Id,
                                Chatid = Chat_Id,
                                Firstname = list.User.FirstName
                            };
                            try
                            {
                                var check = Add_or_update_admins.AdminTables.Single(x => x.AdminId == CheckAdmins.AdminId); // Проверяем, есть ли админ(id) в бд
                                if (check.Chatid != Chat_Id)
                                {
                                    check.ChatId2 = Chat_Id; // Если есть и в другом чате этот же человек тоже админ, записывает в другой столбец для ChatId2
                                    Add_or_update_admins.SaveChanges();
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                Add_or_update_admins.AdminTables.Add(CheckAdmins); // Записываем экземпляр админа впервые
                                Add_or_update_admins.SaveChanges();
                            }
                        }
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"Список админов обновлен:\n{sb}", ParseMode.Markdown);
                    }

                    if (message.Text == "/update_admins@terpilla_bot" && admin2.Status == ChatMemberStatus.Member && Chat_Id != message.From.Id ||
                        message.Text == "/update_admins" && admin2.Status == ChatMemberStatus.Member && Chat_Id != message.From.Id) // Предупреждение что участник не админ
                    {
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"Эта команда доступна только администратору!", ParseMode.Markdown);
                    }

                    if (message.ReplyToMessage != null && message.Text == "!админ")
                    {
                        var SendMessageAdmin = new AdminTable()
                        {
                        };
                        var listAdmins = Add_or_update_admins.AdminTables.Select(x => new { x.AdminId, x.Chatid, x.ChatId2 }).Where(x => x.Chatid == Chat_Id || x.ChatId2 == Chat_Id);

                        foreach (var EachAdmin in listAdmins)
                        {
                            try
                            {
                                await Program.botClient.SendTextMessageAsync(chatId: EachAdmin.AdminId,
                                    $"Пользователь: [{message.From.FirstName}](tg://user?id={message.From.Id})\nпожаловался на участника: " +
                                    $"[{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) из-за сообщения: \n*{message.ReplyToMessage.Text}*\n " +
                                    $"https://t.me/c/{Chat_Id.ToString().Remove(0, 4)}/{message.ReplyToMessage.MessageId}" +
                                    $"\n\n❗️Внимание: чтобы работала ссылка, у вас должна быть *супергруппа(SuperGroup)*"
                                    , ParseMode.Markdown);
                            }
                            catch { }
                        }
                        await Program.botClient.SendTextMessageAsync(Chat_Id, "Жалоба была отправленна администраторам", replyToMessageId: message.MessageId);
                    }
                }
            }
            catch (Exception) {}
        }
    }
}
