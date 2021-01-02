﻿using Discord.Commands;
using ETHBot.DataLayer;
using ETHBot.DataLayer.Data.Enums;
using ETHDINFKBot.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETHDINFKBot.Modules
{
    [Group("sql")]
    public class SqlModule : ModuleBase<SocketCommandContext>
    {
        [Group("table")]
        public class SqlTableModule : ModuleBase<SocketCommandContext>
        {

            private readonly ILogger _logger = new Logger<DiscordModule>(Program.Logger);
            // help


            // list

            // into <table>

            // query <type> (select, insert, update)

            // draw image?

            [Command("help")]
            public async Task SqlTableHelp()
            {

            }

            [Command("list")]
            public async Task ListTables([Remainder] string commandSql)
            {

            }
            private List<ForeignKeyInfo> GetForeignKeyInfo(DbCommand command, string tableName)
            {
                var list = new List<ForeignKeyInfo>();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            ForeignKeyInfo info = new ForeignKeyInfo()
                            {
                                FromTable = tableName, //reader.GetString(2),
                                FromTableFieldName = reader.GetString(3),
                                ToTable = reader.GetString(2),
                                ToTableFieldName = reader.GetString(4),


                            };
                            //0   0   DiscordServers DiscordServerId DiscordServerId NO ACTION CASCADE NONE

                            list.Add(info);

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                        }

                    }
                }

                return list;
            }

            private DBTableInfo GetTableInfo(DbCommand command, string tableName)
            {
                DBTableInfo dbTableInfo = new DBTableInfo()
                {
                    TableName = tableName,
                    FieldInfos = new List<DBFieldInfo>()
                };

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            DBFieldInfo field = new DBFieldInfo()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2),
                                Nullable = !reader.GetBoolean(3),
                                // df value needed?
                                IsPrimaryKey = reader.GetBoolean(5)

                            };


                            dbTableInfo.FieldInfos.Add(field);

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                        }

                    }
                }

                return dbTableInfo;
            }




            [Command("info")]
            public async Task TableInfoTables()
            {
                try
                {
                    var dbInfos = GetAllDBTableInfos();

                    // TODO dispose with using
                    DrawDbSchema drawDbSchema = new DrawDbSchema(dbInfos);
                    drawDbSchema.DrawAllTables();

                    var stream = drawDbSchema.GetStream();
                    Context.Channel.SendFileAsync(stream, "test.png");

                    drawDbSchema.Dispose();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }






            private List<DBTableInfo> GetAllDBTableInfos()
            {
                List<string> tableList = new List<string>()
                {
                    "CommandTypes",
                    "CommandStatistics",
                    "DiscordMessages",
                    "DiscordChannels",
                    "DiscordServers",
                    "BannedLinks",

                    "PingStatistics",
                    "DiscordUsers",
                    "SavedMessages",
                    "RantMessages",
                    "RantTypes",
                    "BotChannelSettings",

                    "SubredditInfos",
                    "RedditPosts",
                    "RedditImages",
                    "EmojiStatistics",
                    "EmojiHistory",
                };


                //;$"PRAGMA table_info('{item}')";
                //PRAGMA foreign_key_list('DiscordChannels');

                string text = "";
                string header = "";

                List<DBTableInfo> DbTableInfos = new List<DBTableInfo>();
                List<List<ForeignKeyInfo>> ForeignKeyInfos = new List<List<ForeignKeyInfo>>();

                using (var context = new ETHBotDBContext())
                {
                    foreach (var item in tableList)
                    {
                        using (var command = context.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = $"PRAGMA table_info('{item}')";
                            context.Database.OpenConnection();
                            if (item == "EmojiStatistics")
                            {
                                //TODO workaround until graphs drawing is done
                                //DbTableInfos.Add(new DBTableInfo());
                            }
                            DbTableInfos.Add(GetTableInfo(command, item));
                        }

                        using (var command = context.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = $"PRAGMA foreign_key_list('{item}')";
                            context.Database.OpenConnection();

                            ForeignKeyInfos.Add(GetForeignKeyInfo(command, item));
                        }
                    }
                }







                foreach (var item in DbTableInfos)
                {
                    text += "**" + item.TableName + "**" + Environment.NewLine;

                    if (item.FieldInfos == null)
                        item.FieldInfos = new List<DBFieldInfo>();

                    foreach (var item2 in item.FieldInfos)
                    {

                        // TODO cleanup
                        foreach (var ForeignKeyInfo in ForeignKeyInfos)
                        {
                            foreach (var fk in ForeignKeyInfo)
                            {
                                if (item2.Name == fk.FromTableFieldName && item.TableName == fk.FromTable)
                                {
                                    item2.IsForeignKey = true;
                                    item2.ForeignKeyInfo = fk;
                                }
                            }
                        }

                        string isPkFk = "";

                        if (item2.IsPrimaryKey)
                            isPkFk = "PK";
                        else if (item2.IsForeignKey)
                            isPkFk = $"FK to {item2.ForeignKeyInfo.ToTable}";



                        text += $"    {item2.Name} ({item2.Type}) {isPkFk} {(item2.Nullable ? "NULLABLE" : "NOT NULLABLE")}" + Environment.NewLine;
                    }

                    text += Environment.NewLine;

                    if (text.Length > 1500)
                    {
                        //Context.Channel.SendMessageAsync(text, false);
                        text = "";
                    }

                }
                return DbTableInfos;

            }
        }


        private bool ContainsForbiddenQuery(string command)
        {
            List<string> forbidden = new List<string>()
            {
                "alter",
                "analyze",
                "attach",
                "transaction",
                "comment",
                "commit",
                "create",
                "delete",
                "detach",
                "database",
                "drop",
                "insert",
                "pragma",
                "reindex",
                "release",
                "replace",
                "rollback",
                "savepoint",
                "update",
                "upsert",
                "vacuum"
            };

            foreach (var item in forbidden)
            {
                if (command.ToLower().Contains(item.ToLower()))
                    return true;
            }

            return false;
        }


        // TODO DUPLICATE REMOVE
        private bool AllowedToRun(BotPermissionType type)
        {
            var channelSettings = DatabaseManager.Instance().GetChannelSetting(Context.Message.Channel.Id);
            if (Context.Message.Author.Id != Program.Owner
                && !((BotPermissionType)channelSettings?.ChannelPermissionFlags).HasFlag(type))
            {
#if DEBUG
                Context.Channel.SendMessageAsync("blocked by perms", false);
#endif
                return true;
            }

            return false;
        }

        public async void WorkaroundForTimeoutNotWorking(CancellationTokenSource cts, SqliteConnection connect, SqliteCommand command)
        {
            await Task.Delay(6000);

            if (cts.IsCancellationRequested)
                return;

            Context.Channel.SendMessageAsync("<:pepegun:747783377716904008>", false);

            //connect.Close();
            //command.Cancel();

            throw new TimeoutException("Time is over");
        }

        private async Task<string> GetRowStringFromReader(SqliteDataReader reader, bool getHeader)
        {
            string resultString = "";

            if (getHeader)
            {
                resultString = "**";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    resultString += reader.GetName(i)?.ToString() + "\t";
                }
                resultString = "**";
                resultString += Environment.NewLine + "```";
            }

            // do something with result
            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    var type = reader.GetFieldType(i)?.FullName;
                    var fieldString = "null";

                    if (DBNull.Value.Equals(reader.GetValue(i)))
                    {
                        resultString += fieldString + "\t";
                        continue;
                    }

                    switch (type)
                    {
                        case "System.Int64":
                            fieldString = reader.GetInt64(i).ToString();
                            break;

                        case "System.String":
                            fieldString = reader.GetValue(i).ToString()?.Replace("`", "");
                            break;

                        default:
                            fieldString = $"{type} is unknown";
                            break;
                    }

                    resultString += fieldString + "\t";
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            resultString += Environment.NewLine;

            return resultString;
        }


        [Command("query")]
        public async Task Sql([Remainder] string commandSql)
        {
            if (AllowedToRun(BotPermissionType.EnableType2Commands))
                return;

            try
            {
                SqlCommand(commandSql);
            }
            catch (Exception ex)
            {
                Context.Channel.SendMessageAsync("Is thia all you got <:kekw:768912035928735775>", false);
            }
        }

        private async void SqlCommand(string commandSql)
        {
            // TODO HELP
            var author = Context.Message.Author;
            if (author.Id != ETHDINFKBot.Program.Owner)
            {
                //Context.Channel.SendMessageAsync("Dont you dare to think you will be allowed to use this command https://tenor.com/view/you-shall-not-pass-lord-of-the-ring-gif-5234772", false);
                //return;
            }

            // TODO Exclude owner

            if (ContainsForbiddenQuery(commandSql) && author.Id != ETHDINFKBot.Program.Owner)
            {
                Context.Channel.SendMessageAsync("Dont you dare to think you will be allowed to use this command https://tenor.com/view/you-shall-not-pass-lord-of-the-ring-gif-5234772", false);
                return;
            }
            try
            {
                bool header = false;
                string resultString = "";
                int rowCount = 0;

                int maxRows = 25;

                CancellationTokenSource cts = new CancellationTokenSource();

                using (var connection = new SqliteConnection(Program.ConnectionString))
                {
                    using (var command = new SqliteCommand(commandSql, connection))
                    {
                        command.CommandTimeout = 5;
                        connection.Open();
                        WorkaroundForTimeoutNotWorking(cts, connection, command);

                        var reader = await command.ExecuteReaderAsync();
                        cts.Cancel();

                        while (reader.Read())
                        {
                            if (rowCount < maxRows || resultString.Length < 2000)
                            {
                                string line = await GetRowStringFromReader(reader, !header);
                                resultString += line;
                                header = true;
                            }
                            rowCount++;
                        }
                    }
                }

                if (resultString.Length > 1950)
                    resultString = resultString.Substring(0, 1950);

                resultString += "```";

                Context.Channel.SendMessageAsync(resultString + Environment.NewLine + $"{rowCount} Row(s) affected", false);
            }
            catch (Exception ex)
            {
                Context.Channel.SendMessageAsync("Error: " + ex.Message, false);
            }
        }
    }

}
