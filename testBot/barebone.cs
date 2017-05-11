using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace testBot
{
    class barebone
    {
        DiscordClient discord;

        public barebone()
        {
            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '~';
                x.AllowMentionPrefix = true;
            });

            var commands = discord.GetService<CommandService>();

            commands.CreateCommand("hello")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("Sup fam!");
                });

            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("MzA5ODk4MjU3NDg2NzA4NzM5.C-2JKg.zMXiDWNe-pgqt0V9uT05wgLWCUo", TokenType.Bot);
            });

        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}