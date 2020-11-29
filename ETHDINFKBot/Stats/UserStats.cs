﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ETHDINFKBot.Stats
{
    public class UserStats
    {
        public int TotalMessages { get; set; }

        public int TotalCommands { get; set; }

        // TODO actually convert it into a dictionary and make it dynamic for new commands
        public int TotalNeko { get; set; }
        public int TotalNekoGif { get; set; }
        public int TotalHolo { get; set; }
        public int TotalWaifu { get; set; }
        public int TotalBaka { get; set; }
        public int TotalSmug { get; set; }
        public int TotalFox { get; set; }
        public int TotalAvatar { get; set; }
        public int TotalNekoAvatar { get; set; }

        // Nekos.Fun & Nekos.life
        public int TotalWallpaper { get; set; }

        // Nekos.Fun
        public int TotalAnimalears { get; set; }
        public int TotalFoxgirl { get; set; }

        public int TotalSearch { get; set; }
    }
}
