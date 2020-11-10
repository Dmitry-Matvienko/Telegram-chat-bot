using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram_bot
{
    class CountMessage
    {
        // Создание столбцов к таблице, котора будет насчитывать кол-во отправленных сообщений
        
        public int id { get; set; }
        public string FirstName { get; set; }
        public int UserId { get; set; }
        public int Counter { get; set; }
    }
}
