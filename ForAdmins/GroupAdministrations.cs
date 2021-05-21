using System;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;

namespace Telegram_bot.ForAdmins
{
    class GroupAdministrations
    {
        public static async void UserControl(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            long Chat_Id = e.Message.Chat.Id;

            if (message.Text is null)
            {
                return;
            }

            try
            {
                ChatMember admin = new ChatMember();
                ChatPermissions chatPermissions = new ChatPermissions();
                admin = Program.botClient.GetChatMemberAsync(Chat_Id, userId: message.From.Id).Result; // Информация об одном из участников чата

                if (message.ReplyToMessage != null && String.Equals(message.Text, "Бан", StringComparison.CurrentCultureIgnoreCase) && admin.CanRestrictMembers == true && e.Message.ReplyToMessage.From.Id != 1234567890
                 || message.ReplyToMessage != null && String.Equals(message.Text, "Бан", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Creator)
                { // бан пользователю, которому был реплай (и поставить id бота чтобы администрация не забанила бота)
                    await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}) забанил участника [{message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) *({e.Message.ReplyToMessage.From.Id})*", parseMode: ParseMode.Markdown);
                    await Program.botClient.KickChatMemberAsync(Chat_Id, userId: e.Message.ReplyToMessage.From.Id);
                }

                if (message.ReplyToMessage != null && String.Equals(message.Text, "Кик", StringComparison.CurrentCultureIgnoreCase) && admin.CanRestrictMembers == true && message.ReplyToMessage.From.Id != 1238720093 ||
                    message.ReplyToMessage != null && String.Equals(message.Text, "Кик", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Creator)
                {
                    await Program.botClient.SendTextMessageAsync(message.Chat.Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}) кикнул участника [{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id})", parseMode: ParseMode.Markdown);
                    await Program.botClient.KickChatMemberAsync(message.Chat.Id, userId: message.ReplyToMessage.From.Id); // бан пользователЮ, которому был реплай
                    await Program.botClient.UnbanChatMemberAsync(message.Chat.Id, userId: message.ReplyToMessage.From.Id); // разбан того же пользователя
                }
            }
            catch (ApiRequestException) { await Program.botClient.SendTextMessageAsync(message.Chat.Id, $"Почему ты такой злой?!"); }
            catch (Exception) { }

            try
            {
                ChatMember admin = new ChatMember();
                admin = Program.botClient.GetChatMemberAsync(Chat_Id, userId: message.From.Id).Result; // Информация об одном из участников чата
                ChatPermissions chatPermissions = new ChatPermissions();

                if (message.ReplyToMessage != null && message.Text.StartsWith(".мут", StringComparison.CurrentCultureIgnoreCase) && admin.CanRestrictMembers == true
                 || message.ReplyToMessage != null && message.Text.StartsWith(".мут", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Creator)
                {// мут пользователю, которому был реплай

                    String[] mute = message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    uint mutes = Convert.ToUInt32(mute[^1]);
                    if (mutes >= 1 && mutes <= 527040)
                    {
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}) дал мут участнику [{message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) на {mute[mute.Length - 1]} минут *({e.Message.ReplyToMessage.From.Id})*", parseMode: ParseMode.Markdown);
                        await Program.botClient.RestrictChatMemberAsync(Chat_Id, userId: e.Message.ReplyToMessage.From.Id,
                                permissions: chatPermissions, untilDate: DateTime.UtcNow.AddMinutes(mutes));
                    }
                    else if (mutes > 527040)
                    {
                        await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.From.FirstName}](tg://user?id={message.From.Id}) дал навсегда мут участнику [{message.ReplyToMessage.From.FirstName}](tg://user?id={e.Message.ReplyToMessage.From.Id}) *({e.Message.ReplyToMessage.From.Id})*", parseMode: ParseMode.Markdown);
                        await Program.botClient.RestrictChatMemberAsync(Chat_Id, userId: e.Message.ReplyToMessage.From.Id,
                                permissions: chatPermissions, untilDate: DateTime.UtcNow.AddMinutes(mutes));
                    }
                }

                else if (message.Text.StartsWith(".фри", StringComparison.CurrentCultureIgnoreCase) && admin.CanRestrictMembers == true ||
                         message.Text.StartsWith(".фри", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Creator)
                { // снимаем мут с пользователя введя его id
                    ChatPermissions unMuteUser = new ChatPermissions
                    {
                        CanInviteUsers = true,
                        CanSendMediaMessages = true,
                        CanSendMessages = true,
                        CanSendOtherMessages = true,
                        CanSendPolls = true
                    };
                    String[] DropString = message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int unMute = Convert.ToInt32(DropString[^1]); // User ID

                    await Program.botClient.SendTextMessageAsync(message.Chat.Id, $"С участника с id {unMute} были сняты ограничения");
                    await Program.botClient.RestrictChatMemberAsync(message.Chat.Id, permissions: unMuteUser, userId: unMute);
                }
            }
            catch (ApiRequestException)
            { }
            catch (FormatException)
            { await Program.botClient.SendTextMessageAsync(message.Chat.Id, $"Некорректное количестно минут!"); }
            catch (Exception ex) when (ex.Message.Contains("large"))
            { await Program.botClient.SendTextMessageAsync(message.Chat.Id, $"Некорректное количестно минут!"); }

            #region Give admin
            try
            {
                var admin = Program.botClient.GetChatMemberAsync(Chat_Id, userId: message.From.Id).Result; // Информация об одном из участников чата
                if (message.ReplyToMessage != null && admin.Status == ChatMemberStatus.Creator
                 || message.ReplyToMessage != null && admin.CanPromoteMembers == true)
                {
                    switch (message.Text)
                    {
                        case "Разрешить удаление":
                            await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) теперь может удалять сообщения", parseMode: ParseMode.Markdown);
                            await Program.botClient.PromoteChatMemberAsync(Chat_Id, userId: message.ReplyToMessage.From.Id,
                                   canDeleteMessages: true); break;
                        case "Разрешить мут":
                            await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) теперь может давать мут и банить участников чата", parseMode: ParseMode.Markdown);
                            await Program.botClient.PromoteChatMemberAsync(Chat_Id, userId: message.ReplyToMessage.From.Id,
                                   canRestrictMembers: true); break;
                        case "Разрешить закреп":
                            await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) теперь может закреплять сообщения", parseMode: ParseMode.Markdown);
                            await Program.botClient.PromoteChatMemberAsync(Chat_Id, userId: message.ReplyToMessage.From.Id,
                                   canPinMessages: true); break;
                        case "Разрешить инвайт":
                            await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) теперь может приглашать юзеров", parseMode: ParseMode.Markdown);
                            await Program.botClient.PromoteChatMemberAsync(Chat_Id, userId: message.ReplyToMessage.From.Id,
                                   canInviteUsers: true); break;
                        case "Дать админку":
                            await Program.botClient.SendTextMessageAsync(Chat_Id, $"[{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) теперь админ", parseMode: ParseMode.Markdown);
                            await Program.botClient.PromoteChatMemberAsync(Chat_Id, userId: message.ReplyToMessage.From.Id,
                                   canRestrictMembers: true,
                                   canDeleteMessages:  true,
                                   canChangeInfo:      true,
                                   canPinMessages:     true,
                                   canInviteUsers:     true,
                                   canPromoteMembers:  true); break;
                        case "Забрать админку":
                            await Program.botClient.SendTextMessageAsync(Chat_Id, $"У [{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) забрали админку :(", parseMode: ParseMode.Markdown);
                            await Program.botClient.PromoteChatMemberAsync(Chat_Id, userId: message.ReplyToMessage.From.Id,
                                   canRestrictMembers: false,
                                   canDeleteMessages:  false,
                                   canChangeInfo:      false,
                                   canPinMessages:     false,
                                   canInviteUsers:     false,
                                   canPostMessages:    false,
                                   canPromoteMembers:  false); break;
                    }
                }

                if (message.Text != null && message.ReplyToMessage != null && message.Text.StartsWith(".н", StringComparison.CurrentCultureIgnoreCase) && admin.CanPromoteMembers == true
                 || message.Text != null && message.ReplyToMessage != null && message.Text.StartsWith(".н", StringComparison.CurrentCultureIgnoreCase) && admin.Status == ChatMemberStatus.Creator)
                {// Выдача надписи участнику 
                    await Program.botClient.PromoteChatMemberAsync(Chat_Id, userId: message.ReplyToMessage.From.Id, canInviteUsers: true);
                    await Program.botClient.SetChatAdministratorCustomTitleAsync(Chat_Id, userId: message.ReplyToMessage.From.Id, $"{message.Text.Remove(0, 2)}");
                    await Program.botClient.SendTextMessageAsync(Chat_Id, $"Теперь [{message.ReplyToMessage.From.FirstName}](tg://user?id={message.ReplyToMessage.From.Id}) *{message.Text.Remove(0, 2)}*", parseMode: ParseMode.Markdown);
                }
            }
            catch (ApiRequestException) { }
            #endregion
        }
    }
}
