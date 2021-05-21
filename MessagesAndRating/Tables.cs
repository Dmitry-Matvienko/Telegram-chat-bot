using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram_bot.MessagesAndRating
{
    class RatingTable
    {
        //  Создание столбцов к таблице, котора будет насчитывать рейтинга для пользователя
        public int id { get; set; }
        public string FirstName { get; set; }
        public int UserId { get; set; }
        public int rate { get; set; }
        public long? ChatId { get; set; }
    }
    class CountMessageTable
    {
        // Создание столбцов к таблице, котора будет насчитывать кол-во отправленных сообщений
        public int id { get; set; }
        public string FirstName { get; set; }
        public int UserId { get; set; }
        public int Counter { get; set; }
        public string rank { get; set; }
        public long? ChatId { get; set; }
    }
    class SecretWordTable
    {
        public int id { get; set; }
        public string words { get; set; }
    }
}
