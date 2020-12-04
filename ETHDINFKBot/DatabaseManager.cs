﻿using Discord;
using ETHBot.DataLayer;
using ETHBot.DataLayer.Data;
using ETHBot.DataLayer.Data.Discord;
using ETHBot.DataLayer.Data.Enums;
using ETHBot.DataLayer.Data.Reddit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ETHDINFKBot
{
    public class DatabaseManager
    {
        private static DatabaseManager _instance;
        private static object syncLock = new object();
        private readonly ILogger _logger = new Logger<DiscordModule>(Program.Logger);

        public static DatabaseManager Instance()
        {
            lock (syncLock)
            {
                if (_instance == null)
                {
                    _instance = new DatabaseManager();
                }
            }

            return _instance;
        }

        //public ETHBotDBContext ETHBotDBContext;
        protected DatabaseManager()
        {
            //ETHBotDBContext = new ETHBotDBContext();
        }

        public DiscordUser GetDiscordUserById(ulong id)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    return context.DiscordUsers.SingleOrDefault(i => i.DiscordUserId == id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public DiscordUser CreateUser(DiscordUser user)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    context.DiscordUsers.Add(user);
                    context.SaveChanges();
                }

                return GetDiscordUserById(user.DiscordUserId); // since this will rarely happen its fine i guess but we could probably return the original user      
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public DiscordServer GetDiscordServerById(ulong id)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    return context.DiscordServers.SingleOrDefault(i => i.DiscordServerId == id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public DiscordServer CreateDiscordServer(DiscordServer server)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    context.DiscordServers.Add(server);
                    context.SaveChanges();
                }

                return GetDiscordServerById(server.DiscordServerId); // since this will rarely happen its fine i guess but we could probably return the original user
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public DiscordChannel GetDiscordChannel(ulong id)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    return context.DiscordChannels.SingleOrDefault(i => i.DiscordChannelId == id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public DiscordChannel CreateDiscordChannel(DiscordChannel channel)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    context.DiscordChannels.Add(channel);
                    context.SaveChanges();
                }

                return GetDiscordChannel(channel.DiscordChannelId); // since this will rarely happen its fine i guess but we could probably return the original user
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }


        public bool CreateDiscordMessage(DiscordMessage message)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    context.DiscordMessages.Add(message);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }


            // .sql "select ri.Link, ri.IsNSFW from SubredditInfos si left join RedditPosts pp on si.SubredditId = pp.SubredditInfoId left join RedditImages ri on pp.RedditPostId = ri.RedditPostId where ri.Link is not null and pp.IsNSFW = (select RedditImageId % 2 from RedditImages ORDER BY RANDOM() LIMIT 1) and si.SubredditName like '%%' ORDER BY RANDOM() LIMIT 5"


            return true;
        }

        public void GetMessage()
        {
            // todo
        }


        public BannedLink GetBannedLink(string link)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    return context.BannedLinks.FirstOrDefault(i => i.Link == link);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }



        public bool CreateBannedLink(BannedLink bannedLink)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    context.BannedLinks.Add(bannedLink);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
            return true;
        }

        public bool CreateBannedLink(string link, ulong userId)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    var user = GetDiscordUserById(userId);

                    context.BannedLinks.Add(new BannedLink()
                    {
                        Link = link,
                        ByUserId = user.DiscordUserId,
                        ReportTime = DateTimeOffset.Now
                    });
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }

            return true;
        }

        public PingStatistic GetPingStatisticByUserId(ulong userId)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    return context.PingStatistics.SingleOrDefault(i => i.DiscordUser.DiscordUserId == userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public void AddPingStatistic(ulong userId, int count, bool isBot)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    var user = GetDiscordUserById(userId);
                    if (user == null)
                    {
                        // TODO can we still get the user or do we need the user to write atleast once?
                        return;
                    }
                    var stat = GetPingStatisticByUserId(userId);
                    if (stat == null)
                    {
                        context.PingStatistics.Add(new PingStatistic()
                        {
                            DiscordUser = user,
                            PingCount = !isBot ? count : 0,
                            PingCountOnce = !isBot ? 1 : 0,
                            PingCountBot = !isBot ? 0 : count,
                            DiscordUserId = user.DiscordUserId
                        });
                    }
                    else
                    {
                        stat.PingCountOnce += !isBot ? 1 : 0;
                        stat.PingCount += !isBot ? count : 0;
                        stat.PingCountBot += !isBot ? 0 : count;
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public EmojiStatistic GetEmojiStatisticById(ulong id)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    return context.EmojiStatistics.SingleOrDefault(i => i.EmojiId == id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public BotChannelSetting GetChannelSetting(ulong channelId)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    return context.BotChannelSettings.SingleOrDefault(i => i.DiscordChannelId == channelId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public void UpdateChannelSetting(ulong channelId, int permission)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    var channelSetting = GetChannelSetting(channelId);
                    if (channelSetting == null)
                    {
                        context.BotChannelSettings.Add(new BotChannelSetting()
                        {
                            DiscordChannelId = channelId,
                            ChannelPermissionFlags = permission
                        });
                    }
                    else
                    {
                        // TODO reuse object from above
                        context.BotChannelSettings.Single(i => i.DiscordChannelId == channelId).ChannelPermissionFlags = permission;
                    }
                    context.SaveChanges();

                    // TODO better way
                    Program.LoadChannelSettings();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public List<BotChannelSetting> GetAllChannelSettings()
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    return context.BotChannelSettings.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public EmojiStatistic AddEmojiStatistic(EmojiStatistic statistic, int count, bool isReaction, bool isBot)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    var dbEmoji = GetEmojiStatisticById(statistic.EmojiId);
                    if (dbEmoji == null)
                    {
                        context.EmojiStatistics.Add(new EmojiStatistic()
                        {
                            Animated = statistic.Animated,
                            CreatedAt = statistic.CreatedAt,
                            EmojiId = statistic.EmojiId,
                            EmojiName = statistic.EmojiName,
                            Url = statistic.Url,
                            UsedAsReaction = !isBot ? statistic.UsedAsReaction : 0,
                            UsedInText = !isBot ? statistic.UsedInText : 0,
                            UsedInTextOnce = !isBot ? statistic.UsedInTextOnce : 0,
                            UsedByBots = !isBot ? 0 : statistic.UsedByBots,
                        });
                        context.SaveChanges();
                    }
                    else
                    {
                        dbEmoji.UsedAsReaction += !isBot ? statistic.UsedAsReaction : 0;
                        dbEmoji.UsedInText += !isBot ? statistic.UsedInText : 0;
                        dbEmoji.UsedInTextOnce += !isBot ? 1 : 0;// statistic.UsedInTextOnce;
                        dbEmoji.UsedByBots += !isBot ? 0 : statistic.UsedByBots;
                        //context.Entry(dbEmoji).State = EntityState.Modified;
                        //dbEmoji.EmojiInfoId;
                    }

                    context.SaveChanges();

                    //ETHBotDBContext.Entry(dbEmoji).State = EntityState.Detached;

                    var eStat = GetEmojiStatisticById(statistic.EmojiId);

                    context.EmojiHistory.Add(new EmojiHistory()
                    {
                        DateTimePosted = DateTime.Now,
                        Count = count,
                        IsReaction = isReaction,
                        IsBot = isBot,
                        EmojiStatisticId = eStat.EmojiInfoId
                    });

                    //ETHBotDBContext.Entry(dbEmoji).State = EntityState.Detached;
                    context.SaveChanges();

                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }

            return GetEmojiStatisticById(statistic.EmojiId);
        }

        public CommandStatistic GetCommandStatisticById(BotMessageType type, ulong userId)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                    return context.CommandStatistics.SingleOrDefault(i => i.DiscordUserId == userId && i.CommandTypeId == (int)type);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public CommandType GetCommandTypeById(int id)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                return context.CommandTypes.Single(i => i.CommandTypeId == id);
            }
        }
        public void AddCommandStatistic(BotMessageType type, ulong userId)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                var dbCommand = GetCommandStatisticById(type, userId);
                if (dbCommand == null)
                {
                    var dbUser = GetDiscordUserById(userId);// maybe not even needed
                    var commandType = GetCommandTypeById((int)type);

                    context.CommandStatistics.Add(new CommandStatistic()
                    {
                        Count = 1,
                        CommandTypeId = commandType.CommandTypeId,
                        DiscordUserId = userId
                    });
                }
                else
                {
                    dbCommand.Count++;
                }


                context.SaveChanges();
            }
        }

        public CommandStatistic GetTopStatisticByType(BotMessageType type)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                return context.CommandStatistics.AsQueryable().Where(i => i.Type.CommandTypeId == (int)type).OrderByDescending(i => i.Count).First(); // TODO check it works
            }
        }

        public bool SaveMessage(ulong messageId, ulong byDiscordUserId, ulong savedByDiscordUserId, string link, string content)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                try
                {
                    //var user = GetDiscordUserById(byDiscordUserId); // Verify the user is created but should actually be available by this poitn
                    //var saveBy = GetDiscordUserById(savedByDiscordUserId); // Verify the user is created but should actually be available by this poitn
                    //ETHBotDBContext.SaveChanges();
                    var newSave = new SavedMessage()
                    {
                        DirectLink = link,
                        SendInDM = false,
                        ByDiscordUserId = byDiscordUserId,
                        //ByDiscordUser = user,
                        Content = content, // todo attachment
                        MessageId = messageId,
                        SavedByDiscordUserId = savedByDiscordUserId
                        //SavedByDiscordUser = saveBy
                    };

                    //context.DiscordUsers.Attach(user);
                    //context.DiscordUsers.Attach(saveBy);
                    context.SavedMessages.Add(newSave);
                    context.SaveChanges();

                }
                catch (Exception ex)
                {

                }
            }

            return true;
        }

        public void SetAllSubredditsStatus(bool status = false)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                var subreddits = context.SubredditInfos.AsQueryable().Where(i => i.IsScraping != status);
                foreach (var subreddit in subreddits)
                {
                    subreddit.IsScraping = status;
                }

                context.SaveChanges();
            }
        }

        public bool SetSubredditScaperStatus(string subreddit, bool status)
        {
            try
            {
                using (ETHBotDBContext context = new ETHBotDBContext())
                {
                    var subredditInfo = GetSubreddit(subreddit);
                    subredditInfo.IsScraping = status;

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }


        public SubredditInfo GetSubreddit(string subreddit)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                return context.SubredditInfos.SingleOrDefault(i => i.SubredditName == subreddit);
            }
        }

        public List<SubredditInfo> GetSubredditsByStatus(bool status = true)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                return context.SubredditInfos.AsQueryable().Where(i => i.IsScraping == status).ToList();
            }
        }

        public RedditPost GetRedditPostById(int redditPostId)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                return context.RedditPosts.SingleOrDefault(i => i.RedditPostId == redditPostId);
            }
        }

        public string GetRandomImage(string subreddit)
        {
            using (ETHBotDBContext context = new ETHBotDBContext())
            {
                Random r = new Random();


                var subredditInfo = GetSubreddit(subreddit);


                var posts = context.RedditPosts.AsQueryable().Where(i => i.SubredditInfo.SubredditId == subredditInfo.SubredditId).OrderBy(i => r.Next(0, 1000));


                //.First().RedditImages.First().Link; // I know this is performance garbage but its 1 AM so fuck you well slept future me who thinks is smarter than my past me
                return posts.First().RedditImages.First().Link;
            }
        }
        // TODO EXCEPTION LOGGING


        // TODO Get saved message

    }
}
