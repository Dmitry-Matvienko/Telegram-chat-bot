using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram_bot
{
    class Rate
    {
        //  Создание столбцов к таблице, котора будет насчитывать рейтинга для пользователя

        public int id { get; set; }
        public string _FirstName { get; set; }
        public int UserId { get; set; }
        public int rate { get; set; }
    }
}
