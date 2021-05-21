using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;

namespace Telegram_bot.ChatAction
{
    class ChatActions
    {
        public static async void WelcomesAndChanges(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long Chat_Id = e.Message.Chat.Id;
            string First_Name = e.Message.From.FirstName;
            try
            {
                if (message?.Type == MessageType.ChatMembersAdded && message.NewChatMembers[0].Id != 1234567890) // Приветствие для всех
                { // TODO: вставить id бота
                    var deleteMessage = await Program.botClient.SendTextMessageAsync(Chat_Id,
                    $"[{message.NewChatMembers[0].FirstName}](tg://user?id={message.NewChatMembers[0].Id}), приветствуем тебя в {message.Chat.Title} чате!\n❗️Заметил(а) нарушение?\nНапиши в чате - !админ❗️",
                    parseMode: ParseMode.Markdown, replyToMessageId: message.MessageId);
                    await Program.botClient.DeleteMessageAsync(Chat_Id, messageId: message.MessageId);
                    await Task.Delay(TimeSpan.FromSeconds(20));
                    await Program.botClient.DeleteMessageAsync(Chat_Id, messageId: deleteMessage.MessageId);
                }

                else if (message?.Type == MessageType.ChatMembersAdded && message.NewChatMembers[0].Id == 1238720093) // Бота добавили в чат
                { // TODO: вставить id бота
                    await Program.botClient.SendTextMessageAsync(Chat_Id, $"Всем привет!\nЯ новенький в вашем чате\nГлавная моя задача - помочь администрации и развлекать пользователей\nБолее детальная инструкция использования: " +
                          $"/help@terpilla_bot");
                }

                else if (message?.Type == MessageType.ChatMemberLeft) // Участник покидает чат
                {
                    await Program.botClient.SendTextMessageAsync(Chat_Id, $" Еще один участник - [{e.Message.LeftChatMember.FirstName}](tg://user?id={message.LeftChatMember.Id}) слился", parseMode: ParseMode.Markdown);
                }

                else if (message?.Type == MessageType.ChatPhotoChanged) // Участник меняет фотку чата
                {
                    await Program.botClient.SendTextMessageAsync(Chat_Id, First_Name + ", и зачем ты это сделал?");
                }

                else if (message?.Type == MessageType.ChatTitleChanged) // Участник меняет название чата
                {
                    await Program.botClient.SendTextMessageAsync(Chat_Id, First_Name + ", кто тебе дал право это делать?");
                }
            }
            catch (Exception) { }
        }
    }
}
