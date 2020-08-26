using System;
using System.Collections.Generic;
using System.IO;

namespace Telegram_chat_bot
{
    class ChoosePhoto
    {
        public Stream Save { get; set; }
        public Stream SavePhoto()
        {
            Stream Photo1 = File.OpenRead(@"C:.jpg");
            Stream Photo2 = File.OpenRead(@"C:.jpg");
            Stream Photo3 = File.OpenRead(@"C:.jpg");
            Stream Photo4 = File.OpenRead(@"C:.jpg");

            List<Stream> list = new List<Stream>()
                    {
                        {Photo1 },
                        {Photo2 },
                        {Photo3 },
                        {Photo4},
                    };

            var Random = new Random();
            int Index = Random.Next(list.Count);
            Save = list[Index];
            return Save;
        }
    }
}
