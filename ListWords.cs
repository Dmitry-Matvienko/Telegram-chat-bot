using System;
using System.Text;

namespace Telegram_chat_bot
{
    class ListWords
    {
        public static string saveReturnWord { get; private set; }
        public string ReturnWord()
        {
            string List_Words = @"C:\.txt";
            var Random = new Random();
            string[] Random_Word = System.IO.File.ReadAllLines(List_Words, Encoding.UTF8);
            string Out_word = Random_Word[Random.Next(Random_Word.Length)];
            saveReturnWord = Out_word;
            return Out_word;
        }
        public string SaveReturnWord()
        {
            return saveReturnWord;
        }
    }
}
