using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Linq;

namespace Telegram_bot.RestrictMedia
{
    class RestrictMediaTable
    {
        public int id { get; set; }
        public string MediaId { get; set; }
        public long Chat_Id { get; set; }
    }
    class Restrict
    {
        public static async void RestrictsMedia(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long Chat_Id = e.Message.Chat.Id;

            if (message.Type == MessageType.Sticker)
            {
                using (var restrictSticker = new DataBaseBot())
                {
                    var stiker = new RestrictMediaTable()
                    {
                        MediaId = message.Sticker.FileUniqueId, // уникальный id стикера в стикерпаке(если этот стикер в другом стикерпаке, id у него будет другой)
                        Chat_Id = message.Chat.Id
                    };
                    try
                    {
                        restrictSticker.restrictMediaTables.Single(x => x.MediaId == stiker.MediaId && x.Chat_Id == stiker.Chat_Id);// проверяем, есть ли стикер в бд
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}), этот стикер запрещен", parseMode: ParseMode.Markdown);
                        await Program.botClient.DeleteMessageAsync(Chat_Id, messageId: message.MessageId);
                    }
                    catch { }
                }
            }

            if (message.Type == MessageType.Document)
            {
                using (var restrictGif = new DataBaseBot())
                {
                    var gif = new RestrictMediaTable()
                    {
                        MediaId = message.Document.FileUniqueId,
                        Chat_Id = message.Chat.Id
                    };
                    try
                    {
                        restrictGif.restrictMediaTables.Single(x => x.MediaId == gif.MediaId && x.Chat_Id == gif.Chat_Id); // проверяем есть ли гиф в бд
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}), эта гиф запрещена", parseMode: ParseMode.Markdown);
                        await Program.botClient.DeleteMessageAsync(Chat_Id, messageId: message.MessageId);
                    }
                    catch { }
                }
            }

            var admin = Program.botClient.GetChatMemberAsync(Chat_Id, userId: message.From.Id).Result;
            if (message.ReplyToMessage != null && message.ReplyToMessage.Type == MessageType.Sticker && String.Equals(message.Text, ".с", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Administrator
             || message.ReplyToMessage != null && message.ReplyToMessage.Type == MessageType.Sticker && String.Equals(message.Text, ".с", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Creator)
            {
                using (var restrict = new DataBaseBot())
                {
                    var restrictMedia = new RestrictMediaTable()
                    {
                        MediaId = message.ReplyToMessage.Sticker.FileUniqueId,
                        Chat_Id = message.Chat.Id
                    };
                    try
                    {
                        restrict.restrictMediaTables.Single(x => x.MediaId == restrictMedia.MediaId && x.Chat_Id == restrictMedia.Chat_Id); // Проверяем наличие стикера в чс в определенном чате(если его хотят занести повторно)
                        await Program.botClient.SendTextMessageAsync(Chat_Id, "Этот стикер уже в бане");

                    }
                    catch (InvalidOperationException)
                    {
                        restrict.restrictMediaTables.Add(restrictMedia); // Заносим стикер в чс
                        restrict.SaveChanges();
                        await Program.botClient.SendTextMessageAsync(Chat_Id, "Стикер занесен в черный список");
                        await Program.botClient.DeleteMessageAsync(Chat_Id, messageId: message.ReplyToMessage.MessageId);
                    }
                }
            }

            if (message.ReplyToMessage != null && message.ReplyToMessage.Type == MessageType.Document && String.Equals(message.Text, ".г", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Administrator
             || message.ReplyToMessage != null && message.ReplyToMessage.Type == MessageType.Document && String.Equals(message.Text, ".г", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Creator)
            {
                using (var restrict = new DataBaseBot())
                {
                    var restrictMedia = new RestrictMediaTable()
                    {
                        MediaId = message.ReplyToMessage.Document.FileUniqueId,
                        Chat_Id = message.Chat.Id
                    };
                    try
                    {
                        restrict.restrictMediaTables.Single(x => x.MediaId == restrictMedia.MediaId && x.Chat_Id == restrictMedia.Chat_Id);
                        await Program.botClient.SendTextMessageAsync(Chat_Id, "Эта гиф уже в бане");
                    }
                    catch (InvalidOperationException)
                    {
                        restrict.restrictMediaTables.Add(restrictMedia);
                        restrict.SaveChanges();
                        await Program.botClient.SendTextMessageAsync(Chat_Id, "Гиф занесена в черный список");
                        await Program.botClient.DeleteMessageAsync(Chat_Id, messageId: message.ReplyToMessage.MessageId);
                    }
                }
            }
        }
    }
}
