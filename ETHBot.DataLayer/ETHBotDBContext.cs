﻿using Microsoft.EntityFrameworkCore;
using System;
using ETHBot.DataLayer.Data.Discord;
using ETHBot.DataLayer.Data.Reddit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ETHBot.DataLayer.Data;
using ETHBot.DataLayer.Data.Study;

namespace ETHBot.DataLayer
{
    public class ETHBotDBContext : DbContext
    {
        private static bool _created = false;

        public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().AddFilter((provider, category, logLevel) => { if (logLevel >= LogLevel.Warning) return true; return false; });
        });

        public ETHBotDBContext()
        {
            //dotnet ef migrations add AddRantTables --project ETHBot.DataLayer/  --startup-project ETHDINFKBot/

            if (!_created)
            {
                _created = true;
                //Database.EnsureDeleted();
                //Database.EnsureCreated();
                Database.Migrate();
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            // TODO Setting
#if DEBUG
            optionbuilder.UseLoggerFactory(loggerFactory).UseSqlite(@"Data Source=I:\ETHBot\ETHBot.db").EnableSensitiveDataLogging();
            //optionbuilder.UseLoggerFactory(loggerFactory).UseSqlite(@"Data Source=I:\ETHBot\ETHBot_20210111_122855").EnableSensitiveDataLogging();    
#else
            optionbuilder.UseLoggerFactory(loggerFactory).UseSqlite(@"Data Source=/usr/local/bin/ETHBot/Database/ETHBot.db").EnableSensitiveDataLogging();
#endif

        }

        public DbSet<BannedLink> BannedLinks { get; set; }
        public DbSet<CommandStatistic> CommandStatistics { get; set; }
        public DbSet<CommandType> CommandTypes { get; set; }
        public DbSet<DiscordChannel> DiscordChannels { get; set; }
        public DbSet<DiscordMessage> DiscordMessages { get; set; }
        public DbSet<DiscordServer> DiscordServers { get; set; }
        public DbSet<DiscordUser> DiscordUsers { get; set; }
        public DbSet<PingStatistic> PingStatistics { get; set; }
        public DbSet<SavedMessage> SavedMessages { get; set; }
        public DbSet<BotChannelSetting> BotChannelSettings { get; set; }
        public DbSet<SubredditInfo> SubredditInfos { get; set; }
        public DbSet<RedditPost> RedditPosts { get; set; }
        public DbSet<RedditImage> RedditImages { get; set; }


        // migrate table
        public DbSet<DiscordEmote> DiscordEmotes { get; set; }
        public DbSet<DiscordEmoteHistory> DiscordEmoteHistory { get; set; }
        public DbSet<DiscordEmoteStatistic> DiscordEmoteStatistics { get; set; }


        public DbSet<RantType> RantTypes { get; set; }
        public DbSet<RantMessage> RantMessages { get; set; }
        /*

        // todo reconsider how to import them
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
