using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;
using Tweetinvi;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API;
using Newtonsoft.Json;
using System.Data;
using YoutubeSearch;


//https://discordapp.com/developers/applications/me
//https://discordapp.com/api/oauth2/authorize?client_id=309908194208251904&scope=bot&permissions=0
namespace testBot
{
    class MyBot
    {
        DiscordClient discord;
        CommandService commands;
        
        string textPath = "texts/";

        List<string> picNames;
        List<string> adjList;
        List<string> nounList;
        List<string> verbList;
        List<string> expList;
        List<string> introList;
        List<string> laughList;
        List<string> heroList;
        List<string> positionList;
        List<string> memeTypesList;

        Dictionary<string[], string> aliases;
        Dictionary<string, string[]> responses;
        Dictionary<string, RexMode> modes;

        Dictionary<string, Dictionary<string, string>> rexDB;

        Dictionary<string, int> reports;

        VideoSearch youtubeSearcher;

        DateTime dtp;
        TimeSpan tsp;
        string devUpdate;
        string comingSoon = "Report Command Fix";

        string mode = "jamie";
        bool userLoadRequst = false;


        Random rnd;

        public MyBot()
        {
            dtp = new DateTime(2017, 5, 11);
            TimeSpan ts = new TimeSpan(12, 30, 0);
            dtp += ts;

            DateTime dtp2 = new DateTime(2017, 5, 11);
            TimeSpan ts2 = new TimeSpan(14, 25, 0);
            dtp2 = dtp;
            dtp2 += ts2;

            devUpdate = "\nI'm flying to Korea!\nDeparture from LAX ETA : " + Math.Round((dtp - DateTime.Now).TotalHours,2) + " Hours!\n"+
                "Arrival in Korea ETA : " + Math.Round((dtp2 - DateTime.Now).TotalHours,2) + " Hours!\n";

            Tweetinvi.Auth.SetUserCredentials("5JFjR7DXgDb4CQE0K1UwRO3Vt", "3i9ynzdJUhpxWkFoCXmuMjHIP19oxMbLlpcbdUUU6HhFMLI3f4", "1561997844-pZyWmSSAewcCVAV8u9IFK3j7iCPKZ7TQwXlfblO", "9faKrZDGS0FwEkJGKT3Xd90uqzVvSIattAuI5r7uVRdqI");
            var zuser = Tweetinvi.User.GetAuthenticatedUser();
            Console.WriteLine("Twitter initializing: " + zuser);
            var imgurclient = new ImgurClient("1a8e14c14351c3b", "ddbf24f6f6a01ad626dea3600ce002cd5ae2d04f");
            //https://api.imgur.com/oauth2/authorize?client_id=1a8e14c14351c3b&response_type=token
            //https://api.imgur.com/3/gallery/search/q?=buzz&q_type=jpeg&q_size_px=500
            //GetImage();
            //var simlBot = new SimlBot();
            //var botUser = simlBot.CreateUser();
            //var chatRequest = new ChatRequest("how are you", RequestType.UserMessage, botUser);
            //var chatResult = simlBot.Chat(chatRequest);
            //var botMessage = chatResult.BotMessage;

            rnd = new Random();
            picNames = new List<string>();
            adjList = new List<string>();
            nounList = new List<string>();
            verbList = new List<string>();
            expList = new List<string>();
            introList = new List<string>();
            laughList = new List<string>();
            heroList = new List<string>();
            positionList = new List<string>();
            memeTypesList = new List<string>();
            aliases = new Dictionary<string[], string>();
            responses = new Dictionary<string, string[]>();
            modes = new Dictionary<string, RexMode>();
            rexDB = new Dictionary<string, Dictionary<string,string>>();
            reports = new Dictionary<string, int>();

            youtubeSearcher = new VideoSearch();

            modes.Add("jamie", new RexMode("jamie", "No auto triggers. No status updates. All functions online.", new string[] {"functions"}));
            modes.Add("active", new RexMode("active", "Occasional auto triggers.", new string[] { "functions","trigger 30" }));
            modes.Add("loud", new RexMode("loud", "Many auto triggers. Status changes.", new string[] { "functions","trigger 60","status" }));
            modes.Add("tooloud", new RexMode("tooloud", "RexBot on Steroids", new string[] { "functions","trigger 100", "status" }));
            modes.Add("cat", new RexMode("cat", "Posts a cat photo for every message, as well as a snarky comment", new string[] { "functions", "trigger 100", "status","cat" }));

            //aliases.Add(new string[] { "Schafer", "henry", "henhen","yrneh" }, "<:henryface:309024772971298816>");
            //aliases.Add(new string[] { "Ryanne", "Schäfer", "guki", "gukimon" }, "<:gukiface:309032781327892480>");
            //aliases.Add(new string[] { "andy" }, "Andy");
            //aliases.Add(new string[] { "Wolfy", "ryan", "ryry","ryanna","wolf" }, "<:ryanface2:308726136681267201>");
            //aliases.Add(new string[] { "Geffo", "geoff", "geoffo", "omni", "omnisexy" }, "<:geoffface:309030675472973834>");
            //aliases.Add(new string[] { "Pash", "pash", "pgod" }, "<:pashface:309032171379621889>");
            //aliases.Add(new string[] { "BonoboCop", "xander", "conductor" }, "<:xanderface:309027227855486986>");
            //aliases.Add(new string[] { "rooster212", "jamie", "yaymie" }, "<:jamieface:308721678442299394>");
            //aliases.Add(new string[] { "CPTOblivious", "nick", "silencer" }, "<:nickface:309026407139246100>");
            //aliases.Add(new string[] { "Rexyrex", "rexy", "rexyrex","adrian" }, "<:adrianface:309027765695545344>");
            //aliases.Add(new string[] { "RayRay", "ray", "rayray", "raydrian" }, "<:rayface:309025821367074817>");

            Console.WriteLine("Starting population...");
            populate(adjList, "adjective.txt");
            populate(nounList, "noun.txt");
            populate(verbList, "verb.txt");
            populate(expList, "statement.txt");
            populate(introList, "intro.txt");
            populate(laughList, "laugh.txt");
            populate(heroList, "heros.txt");
            populate(positionList, "position.txt");
            populate(memeTypesList, "memeType.txt");
            populateResponses();
            populatePicFileNames();
            ParseAliases();
            loadRexDB();
            Console.WriteLine("Done Loading!");

            discord = new DiscordClient(x =>
           {
               x.LogLevel = LogSeverity.Info;
               x.LogHandler = Log;
           });

           discord.UsingCommands(x =>
           {
               x.PrefixChar = '!';
               x.AllowMentionPrefix = true;
           });


            //discord.SetStatus(UserStatus.DoNotDisturb);

            //status updates
            discord.UserUpdated += async (s, e) =>
            {
                if (modes[mode].hasPermission("status"))
                {
                    //var channel = e.Server.FindChannels("status", ChannelType.Text);
                    var mainChannel = e.Server.FindChannels("hoofenpaw_warriors", ChannelType.Text);
                    string name = e.After.Name;
                    string status = e.After.Status.ToString();

                    //foreach (var ch in channel)
                    //{
                    Console.WriteLine(e.After.Name + "'s status has changed from " + e.Before.Status + " to " + e.After.Status + " at " + DateTime.Now.ToString());
                    //}

                    foreach (KeyValuePair<string[], string> entry in aliases)
                    {
                        foreach (string entS in entry.Key)
                        {
                            if (name == entS)
                            {
                                foreach (var ch in mainChannel)
                                {
                                    if (status == "online")
                                        await ch.SendMessage("Welcome back " + aliases[entry.Key] + "!");
                                    if (status == "dnd")
                                        await ch.SendMessage("shhhhhhhhhh! " + aliases[entry.Key] + " doesnt want to be disturbed!");
                                    //I'm so happy to see you :D\nIn case you weren't aware, i've been upgraded significantly...\nPlz type !help to see what i'm capable of!"
                                    //else if (status == "offline")
                                    //await ch.SendMessage("Sad to see you go " + aliases[entry.Key] + "!\nI'll catch you next time buddy :)");
                                }
                            }
                        }
                    }
                }
            };

            //---------------------------------------------
            //AUTO TRIGGERS--------------------------------
            //---------------------------------------------
            discord.MessageReceived += async (s, e) =>
            {

                string stz = e.Message.Text.ToLower();
                string user = e.Message.User.ToString();

                if (!e.Message.IsAuthor && modes[mode].hasPermission("functions"))
                {
                    if (ContainsAny(stz, new string[] { "!meme" }) && stz.Count(x => x == '(') == 3)
                    {
                        int bracketCount = 0;
                        string type = string.Empty;
                        string topText = string.Empty;
                        string botText = string.Empty;

                        for (int i = 0; i < stz.Length; i++)
                        {
                            if (stz[i] == '(' || stz[i] == ')')
                            {
                                bracketCount++;
                            }
                            if (bracketCount == 3)
                            {
                                if (stz[i] != '(')
                                    topText += stz[i];
                            }
                            if (bracketCount == 5)
                            {
                                if (stz[i] != '(')
                                    botText += stz[i];
                            }
                            if (bracketCount == 1)
                            {
                                if (stz[i] != ')' && stz[i] != '(')
                                    type += stz[i];
                            }
                        }
                        topText = topText.Replace(' ', '-');
                        botText = botText.Replace(' ', '-');
                        Console.WriteLine(topText);
                        Console.WriteLine(botText);
                        await e.Channel.SendMessage("https://memegen.link/" + type + "/" + topText + "/" + botText + ".jpg");
                    }
                }
                if (roll(modes[mode].getTriggerChance())){
                    if (!e.Message.IsAuthor)
                    {

                        int wcount = stz.Count(x => x == 'w');
                        int acount = stz.Count(x => x == '@');

                        if (e.Message.IsTTS && ((wcount > 0.4 * stz.Length && !stz.Contains('a')) || acount > 0.4 * stz.Length || stz.Contains("tata"))  && stz.Length>=3)
                        {
                            await e.Channel.SendMessage("TTS ABUSER DETECTED");
                        }

                        foreach (KeyValuePair<string[], string> entry in aliases)
                        {
                            foreach (string entS in entry.Key)
                            {
                                if (stz.Contains(entS)) {
                                    //await e.Channel.SendMessage(user + " mentioned something about " + aliases[entry.Key] + "...");
                                    //await e.Channel.SendTTSMessage("Here is a recent tweet related to " + aliases[entry.Key] + " : " + getTweet(aliases[entry.Key]));
                                }
                            }
                        }

                        if (ContainsAny(stz,new string[] { "cat","kitty","kitten","caat","caaat","caaaat","caaaaat","meow"}) || modes[mode].hasPermission("cat"))
                        {
                            string jsonStr = makeHTTPRequest("http://random.cat/meow");
                            dynamic dynObj = JsonConvert.DeserializeObject(jsonStr);
                            string urlStr = dynObj.file;
                            await e.Channel.SendMessage("DID I HEAR CAT???");
                            await e.Channel.SendMessage(urlStr);                            

                        }
                        //if (ContainsAny(stz, new string[] { "eminem", "one shot", "spaghetti"}))
                        //{
                        //    await e.Channel.SendFile("pics/" + "eminem.jpg");
                        //    await e.Channel.SendTTSMessage("PALMS SPAGHETTI KNEAS WEAK ARM SPAGHETTI THERES SPAGHETTI ON HIS SPAGHETTI ALREADY, MOMS SPAGHETTI");
                        //}

                        foreach (KeyValuePair<string, string[]> trigger in responses)
                        {
                            if (stz.Contains(trigger.Key))
                            {
                                int rz = rnd.Next(0, responses[trigger.Key].Length);
                                await e.Channel.SendMessage(responses[trigger.Key][rz]);
                            }
                        }
                    }
                }                    
            };

           commands = discord.GetService<CommandService>();
            AddPicCommands();
            AddTextCommands();
            AddHTTPCommands();
           discord.ExecuteAndWait(async () =>
           {
               await discord.Connect("MzA5OTA4MTk0MjA4MjUxOTA0.C-2QGA.P9YuqHOZMOngv1bRndH3UoVZo4Y", TokenType.Bot);
           });
        }

        public async Task GetImage()
        {
            try
            {
                var client = new ImgurClient("1a8e14c14351c3b", "ddbf24f6f6a01ad626dea3600ce002cd5ae2d04f");
                //var endpoint = new ImageEndpoint(client);
                //var image = await endpoint.GetImageAsync("BzxWt");
                var endpointz = new GalleryEndpoint(client);
                var images = await endpointz.SearchGalleryAsync("cat");
                foreach (var item in images)
                {
                    Console.Write("1."+item.ToJson());
                    
                }
            }
            catch (ImgurException imgurEx)
            {
                Console.Write("An error occurred getting an image from Imgur.");
                Console.Write(imgurEx.Message);
            }
        }

        public string getTweet(string search)
        {
            string finalTweet = "No Tweet Found (Rex Tweet error plz msg Adrian)";
            var tweetTest = Search.SearchTweets(search);            

            foreach(var item in tweetTest)
            {
                if (!(item.ToString().Contains('@')))
                    finalTweet = item.ToString();
            }
            return finalTweet;
        }

        public static bool ContainsAny(string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }
            return false;
        }

        private void Discord_UserJoined(object sender, UserEventArgs e)
        {
            throw new NotImplementedException();
        }

        private string stripName(string name)
        {
            return name.Split('#')[0];
        }

        private void AddPicCommands()
        {
            commands.CreateCommand("sup")
                .Description("Get a pic with an appropriate description")
                .Alias(new string[] { "pic", "picplz"})
                .Do(async (e) =>
                {
                    int r = rnd.Next(picNames.Count);
                    string picToString = picNames[r];
                    await e.Channel.SendFile("friendpics/" + picToString);
                    await e.Channel.SendTTSMessage(commentOnPic());
                });
            commands.CreateCommand("kappa")
                .Description("KAPPA FACE")
                .Do(async (e) =>
                {
                        await e.Channel.SendFile("pics/" + "Kappahd.png");
                });
            commands.CreateCommand("doge")
                .Description("DOGE")
                .Do(async (e) =>
                {
                    string topText = "wow! such " + getWord(adjList);
                    string botText = "much " + getWord(nounList);
                    topText = processTextForMeme(topText);
                    botText = processTextForMeme(botText);
                    await e.Channel.SendMessage("https://memegen.link/" + "doge" + "/" + topText + "/" + botText + ".jpg");
                    //await e.Channel.SendFile("pics/" + "doge.jpeg");
                });
            commands.CreateCommand("feelsgood")
                .Description("feelsgoodman")
                .Do(async (e) =>
                {
                    await e.Channel.SendFile("pics/" + "feelsgood.png");
                });
            commands.CreateCommand("feelsbad")
                .Description("feelsbadman")
                .Do(async (e) =>
                {
                    await e.Channel.SendFile("pics/" + "feelsbad.png");
                });
            commands.CreateCommand("choo")
                .Alias(new string[] { "feedtrain", "feed","choochoo"})
                .Description("The very definition of this group")
                .Do(async (e) =>
                {
                    await e.Channel.SendFile("pics/" + "choochooz.png");
                });
            commands.CreateCommand("cat")
                .Description("Random cat photo")
                .Do(async (e) =>
                {
                    string jsonStr = makeHTTPRequest("http://random.cat/meow");
                    dynamic dynObj = JsonConvert.DeserializeObject(jsonStr);
                    string urlStr = dynObj.file;
                    await e.Channel.SendMessage(urlStr); 
                });
            commands.CreateCommand("dog")
                .Description("Random dog photo")
                .Do(async (e) =>
                {
                    string jsonStr = makeHTTPRequest("https://random.dog/woof.json");
                    dynamic dynObj = JsonConvert.DeserializeObject(jsonStr);
                    string urlStr = dynObj.url;
                    await e.Channel.SendMessage(urlStr);
                });
            commands.CreateCommand("eminem")
                .Description("Eminem")
                .Do(async (e) =>
                {
                    await e.Channel.SendFile("pics/" + "eminem.jpg");
                    await e.Channel.SendTTSMessage("PALMS SPAGHETTI KNEAS WEAK ARM SPAGHETTI THERES SPAGHETTI ON HIS SPAGHETTI ALREADY, MOMS SPAGHETTI");
                });
        }

        private void AddHTTPCommands()
        {
            commands.CreateCommand("youtube")
                .Description("Search youtube (only 1 word queries for now)")
                .Parameter("query",ParameterType.Required)
                .Do(async (e) =>
                {
                    List<Video> list = new List<Video>();
                    foreach(var item in youtubeSearcher.SearchQuery(e.GetArg("query"), 1))
                    {
                        Video video = new testBot.Video();
                        video.Title = item.Title;
                        video.Author = item.Author;
                        video.url = item.Url;
                        list.Add(video);
                    }
                    int r = rnd.Next(list.Count);
                    
                    await e.Channel.SendMessage(list[r].url);
                });

            commands.CreateCommand("meme")
                .Parameter("help",ParameterType.Optional)
                .Description("Get a random meme background")
                .Do(async (e) =>
                {
                    string option = e.Args[0];
                    if (ContainsAny(option, new string[] { "help","list","type"}))
                    {
                        string res = string.Empty;
                        res += "** - Meme Creation - **\n";
                        res += "```MarkDown\n" + "!meme (type) (Top Line) (Bottom Line)\n```";
                        res += "\n** - Meme Types - **\n" + "```";
                        foreach (string memeType in memeTypesList)
                        {
                            res += memeType + ", ";
                        }
                        res += "```";
                        await e.Channel.SendMessage(res);
                    } else
                    {
                        List<string> results = new List<string>();
                        string jsonStr = makeHTTPRequest("https://api.imgflip.com/get_memes");
                        dynamic dynObj = JsonConvert.DeserializeObject(jsonStr);
                        foreach (var data in dynObj.data.memes)
                        {
                            string urlStr = data.url;
                            results.Add(urlStr);
                        }
                        await e.Channel.SendMessage("***No Argument Error - Generating random meme background:***\n " + getWord(results));
                    }
                });

             commands.CreateCommand("rather")
                .Description("What would you rather do?")
                .Do(async (e) =>
                {
                    string jsonStr = makeHTTPRequest("http://www.rrrather.com/botapi");
                    dynamic dynObj = JsonConvert.DeserializeObject(jsonStr);
                    string urlStr = dynObj.link;
                    string question = dynObj.title;
                    string choiceA = dynObj.choicea;
                    string choiceB = dynObj.choiceb;
                    string votes = dynObj.votes;
                    await e.Channel.SendMessage("```" + question + "\nA. " + choiceA + "\nB. " + choiceB + "\n\n"+votes+ " people voted on this question\n\n```" + "Click here to find out what dino option they chose : " + urlStr);
                });
            commands.CreateCommand("today")
                .Description("What happened today in the past?")
                .Do(async (e) =>
                {
                    string jsonStr = makeHTTPRequest("http://history.muffinlabs.com/date");
                    dynamic dynObj = JsonConvert.DeserializeObject(jsonStr);
                    string date = dynObj.date;
                    List<string> results = new List<string>();
                    foreach (var data in dynObj.data.Events)
                    {
                        string year = data.year;
                        string text = data.text;
                        results.Add("Year " + year + ", "+ date + " : " + text);
                    }
                    await e.Channel.SendMessage(getWord(results));
                });
            commands.CreateCommand("twitter")
                .Parameter("query", ParameterType.Optional)
                .Description("Search for a term on Twitter")
                .Do(async (e) =>
                {
                    string searchTerm = "Dota2";
                    if (e.Args[0] == "")
                    {
                        searchTerm = "Dota2";
                    }
                    else
                    {
                        searchTerm = e.GetArg("query");
                    }
                    await e.Channel.SendMessage(getTweet(searchTerm));
                });
        }

        private string processTextForMeme(string input)
        {
            string res = string.Empty;
            res = input.Replace("-", "--");
            res = res.Replace(' ', '-');
            res = res.Replace("_", "__");            
            res = res.Replace("?", "~q");
            res = res.Replace("%", "~p");
            res = res.Replace("#", "~h");
            res = res.Replace("/", "~s");
            return res;
        }


        private void AddTextCommands()
        {
            commands.CreateCommand("status")
                .Description("Shows the status of the bot")
                .Do(async (e) =>
                {
                    DateTime dateTime = new DateTime(2017, 5, 5);
                    string helpStr = string.Empty;
                    helpStr += "```bash\n" +
                    "RexBot v1.0 made by Rexyrex\n" +
                    "===========================\n";
                    helpStr += "Mode : " + mode + " - " + modes[mode].getDescription() + "\n";
                    helpStr += "Code : " + "1407 lines" + "\n";
                    helpStr += "Age : " + Math.Round((DateTime.Now - dateTime).TotalDays,2) + " days" + "\n";
                    helpStr += "Current Time : " + DateTime.Now.ToString() + "\n";
                    helpStr += "Latest Developer Update : <" + devUpdate + ">\n";
                    helpStr += "Coming Soon : <" + comingSoon + ">\n";
                    helpStr += "```";
                    await e.Channel.SendMessage(helpStr);
                });
            commands.CreateCommand("help")
                .Parameter("alias",ParameterType.Optional)
                .Description("You just used this command you mongie")
                .Do(async (e) =>
                {
                    if(e.Args[0] == "")
                    {
                        string helpStr = string.Empty;
                        helpStr += "```Markdown\n" +
                        "RexBot v1.0 made by Rexyrex\n" +
                        "===========================\n";
                        foreach (Command c in commands.AllCommands)
                        {

                            int spaceCount = 10 - c.Text.Length;
                            string spaces = string.Empty;
                            for (int i = 0; i < spaceCount; i++)
                            {
                                spaces += " ";
                            }
                            helpStr += "⚝" + spaces + c.Text + " : " + c.Description + "\n";
                        }
                        helpStr += "```";
                        await e.Channel.SendMessage(helpStr);
                    } else
                    {
                        string commandName = e.Args[0];
                        foreach (Command c in commands.AllCommands)
                        {
                            if(commandName == c.Text)
                            {
                                string aliasesStr = string.Empty;
                                string parametersStr = string.Empty;
                                foreach (string s in c.Aliases)
                                {
                                    aliasesStr += s + ", ";
                                }
                                foreach (CommandParameter x in c.Parameters)
                                {
                                    parametersStr += "(Name: " + x.Name + ", Type: " + x.Type + ")";
                                }
                                string res = "**Command :** " + c.Text;
                                res += "\n**Aliases:** " + aliasesStr;
                                res += "\n**Category:** " + c.Category;
                                res += "\n**Description:** " + c.Description;
                                res += "\n**Parameters:** " + parametersStr;
                                await e.Channel.SendMessage(res);
                            }

                        }
                    }
                    
                });

            commands.CreateCommand("off")
                .Alias(new string[] { "exit", "quit"})
                .Description("down for maintenance")
                .Do(async (e) =>
                {
                    if(e.User.ToString() == "Rexyrex#5838")
                    {
                        Discord.Message[] messages;
                        messages = await e.Channel.DownloadMessages(1);
                        await e.Channel.DeleteMessages(messages);
                        await e.Channel.SendMessage("```css \nI'm going down for maintenance! brb...\n```");
                        System.Threading.Thread.Sleep(1000);
                        System.Environment.Exit(1);
                    } else
                    {
                        await e.Channel.SendTTSMessage("Nice try " + stripName(e.User.ToString()));
                    }
                    
                });

            commands.CreateCommand("W")
                .Description("has a chance of spamming /tts W W W W W x 10")
                .Do(async (e) =>
                {
                    int randInt = rnd.Next(1,101);
                    int randInt2 = rnd.Next(1, 101);
                    string res = "you rolled " + randInt + " when you should have rolled " + randInt2;
                    res += "\nNo W's for you today " + stripName(e.User.ToString()) + "!";
                    if (randInt == randInt2)
                    {
                        await e.Channel.SendTTSMessage("W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W W, W");
                    }  else
                    {
                        await e.Channel.SendMessage(res);
                    }
                });




            commands.CreateCommand("play")
                .Description("(Dota2) Tells you what hero and role you should play")
                .Parameter("users", ParameterType.Optional)
                .Do(async (e) =>
                {
                    string users = e.GetArg("users");
                    if (users == "")
                    {
                        users = "You";
                    }
                    await e.Channel.SendMessage(users + " should play " + getWord(positionList) + " " + getWord(heroList));
                });
            commands.CreateCommand("hero")
                .Description("(Dota2) Output a random hero")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage(getWord(heroList));
                });

            commands.CreateCommand("roll")
                .Description("roll like a pole")
                .Parameter("n1", ParameterType.Optional)
                .Parameter("n2", ParameterType.Optional)
                .Do(async (e) =>
                {
                    string result = string.Empty;
                    result += stripName(e.User.ToString()) + " rolled ";
                    int num = 0;
                    if (e.Args[0] == "")
                    {
                        num = rnd.Next(0, 100 + 1);
                    } else if(e.Args[1] == ""){
                        num = rnd.Next(0, int.Parse(e.Args[0])+1);
                    } else
                    {
                        num = rnd.Next(int.Parse(e.Args[0]), int.Parse(e.Args[1])+1);
                    }
                    await e.Channel.SendMessage(result + num.ToString());
                });

            commands.CreateCommand("flip")
                .Description("Flip a dino coin")
                .Do(async (e) =>
                {
                    int rand = rnd.Next(0, 2);
                    string userName = stripName(e.User.ToString());
                    if (rand == 1)
                    {
                        await e.Channel.SendMessage(userName + " flipped heads!");
                    } else
                    {
                        await e.Channel.SendMessage(userName + " flipped tails!");
                    }
                    
                });

            commands.CreateCommand("urban")
                .Description("Look up definition of word on urban dictionary")
                .Parameter("word", ParameterType.Required)
                .Do(async (e) =>
                {
                    string users = e.GetArg("word");

                    string jsonStr = makeHTTPRequest("http://api.urbandictionary.com/v0/define?term="+users);
                    dynamic dynObj = JsonConvert.DeserializeObject(jsonStr);
                    List<string> urls = new List<string>();
                    foreach (var data in dynObj.list)
                    {
                        string urlStr = data.definition;
                        urls.Add(urlStr);
                    }
                    await e.Channel.SendMessage(getWord(urls));
                });

            commands.CreateCommand("gif")
                .Description("Search for a gif or random (if no query)")
                .Parameter("query", ParameterType.Optional)
                .Do(async (e) =>
                {
                    string query = string.Empty;
                    string jsonStr = string.Empty;
                    List<string> urls = new List<string>();
                    dynamic dynObj;
                    if (e.Args[0] == "")
                    {
                        jsonStr = makeHTTPRequest("http://api.giphy.com/v1/gifs/random?api_key=dc6zaTOxFJmzC");
                        dynObj = JsonConvert.DeserializeObject(jsonStr);
                        
                        string urlStr = dynObj.data.url;
                        urls.Add(urlStr);
                    } else
                    {
                        query = e.GetArg("query");
                        jsonStr = makeHTTPRequest("http://api.giphy.com/v1/gifs/search?q="+ query +"&api_key=dc6zaTOxFJmzC");
                        dynObj = JsonConvert.DeserializeObject(jsonStr);
                        foreach(var d in dynObj.data)
                        {
                            string ul = d.url;
                            urls.Add(ul);
                            Console.WriteLine(ul);
                            Console.WriteLine("herp derp");
                        }
                        Console.WriteLine("At least were here");   
                    }
                    await e.Channel.SendMessage(getWord(urls));


                });

            commands.CreateCommand("gif2")
                .Description("Post a random trending GIF")
                .Parameter("query", ParameterType.Optional)
                .Do(async (e) =>
                {
                    string url = e.GetArg("query");
                    /*if (url != null && url != "")
                    {
                        Console.WriteLine("ERROR");
                        await e.Channel.SendMessage("No support for search yet" + url);
                    } else*/
                    //{
                    string jsonStr = makeHTTPRequest("http://api.giphy.com/v1/gifs/trending?api_key=dc6zaTOxFJmzC");
                    dynamic dynObj = JsonConvert.DeserializeObject(jsonStr);
                    List<string> urls = new List<string>();

                    foreach (var data in dynObj.data)
                    {
                        string urlStr = data.url;
                        urls.Add(urlStr);
                        Console.WriteLine("adding one url");
                    }
                    Console.WriteLine("done");
                    await e.Channel.SendMessage(getWord(urls));
                    //}
                });

            commands.CreateCommand("mode")
                .Description("Change mode. (silent,quiet,loud)")
                .Parameter("newmode", ParameterType.Required)
                .Do(async (e) =>
                {
                    string userName = e.Message.User.ToString();
                    string reqMode = e.GetArg("newmode");
                    if (userName == "Rexyrex#5838")
                    {
                        if (isMode(reqMode))
                        {
                            mode = reqMode;
                            await e.Channel.SendMessage("RexBot mode changed to " + reqMode);
                        } else if (reqMode == "help" || reqMode == "")
                        {
                            await e.Channel.SendMessage(getAllModesInfo());
                        } else
                        {
                            await e.Channel.SendMessage("***You input an invalid mode!***\n\n*Available modes*:\n"+getAllModesInfo());
                        }                    
                    } else
                    {
                        await e.Channel.SendMessage("I don't listen to scrubs like you");
                    }
                });
            commands.CreateCommand("calc")
                .Description("Calculates the given math equation")
                .Parameter("eq", ParameterType.Required)
                .Do(async (e) =>
                {
                    DataTable dt = new DataTable();
                    var v = dt.Compute(e.GetArg("eq"), "");
                    await e.Channel.SendMessage(e.GetArg("eq") + "=" + v.ToString());
                });

            commands.CreateCommand("purge")
                .Parameter("n", ParameterType.Optional)
                .Description("Not Implemented")
                .Do(async (e) =>
                {
                    int n = 0;
                    if(e.Args[0] == "")
                    {
                        n = 2;
                    } else
                    {
                        n = int.Parse(e.Args[0]);
                    }
                    
                    string userName = e.Message.User.ToString();
                    if (userName == "Rexyrex#5838")
                    {
                        Discord.Message[] messages;
                        messages = await e.Channel.DownloadMessages(n);
                        await e.Channel.DeleteMessages(messages);
                    } else
                    {
                        await e.Channel.SendMessage("Not implemented");
                    }
                        
                });
            commands.CreateCommand("repeat")
                .Parameter("str", ParameterType.Required)
                .Description("Repeats what you say")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage(e.GetArg("str"));
                });
            commands.CreateCommand("aka")
                .Description("Show all aliases of friends")
                .Do(async (e) =>
                {                    
                    await e.Channel.SendMessage(getAliases());
                });
            commands.CreateCommand("img")
                .Parameter("query",ParameterType.Optional)
                .Description("Search for an image on imgur (default:cat)")
                .Do(async (e) =>
                {
                    string query = string.Empty;
                    List<string> urls = new List<string>();

                    if (e.GetArg("query") == "")
                    {
                        query = "cat";
                    } else
                    {
                        query = e.GetArg("query");
                    }

                    
                    string t = makeHTTPRequest2("https://api.imgur.com/3/gallery/search/?q=" + query);


                    dynamic dynObj = JsonConvert.DeserializeObject(t);
                    Console.WriteLine(dynObj);

                    foreach(var data in dynObj.data)
                    {
                        string dt = data.link;
                        urls.Add(dt);
                    }

                    await e.Channel.SendMessage(getWord(urls));
                });

            commands.CreateCommand("save")
                .Parameter("id",ParameterType.Required)
                .Parameter("content", ParameterType.Required)
                .Description("save <id> <string> - Save line in rex db")
                .Do(async (e) =>
                {
                    writeToRexDB(e.User.ToString(), e.GetArg("id"), e.GetArg("content"));                 

                    await e.Channel.SendMessage("Save successful");
                });

            commands.CreateCommand("load")
                .Parameter("id", ParameterType.Required)
                .Description("load <id> - Loads your string saved in <id>")
                .Do(async (e) =>
                {
                    string user = e.User.ToString();
                    string id = e.GetArg("id");

                    
                    await e.Channel.SendMessage(getFromRexDB(user, id));
                });

            commands.CreateCommand("list")
                .Description("load <id> - Loads your string saved in <id>")
                .Do(async (e) =>
                {
                    string user = e.User.ToString();


                    await e.Channel.SendMessage("**Key Value Pairs for " + stripName( user) + "...**\n" + listRexDB(user));
                });
            commands.CreateCommand("report")
                .Parameter("name", ParameterType.Required)
                .Description("report this fool")
                .Do(async (e) =>
                {
                    string name = e.GetArg("name");
                    string user = e.User.ToString();
                    if (reports.ContainsKey(name))
                    {
                        reports[name]++;
                    } else
                    {
                        reports[name] = 1;
                    }

                    await e.Channel.SendMessage("Report successful");
                });
            commands.CreateCommand("allreports")
                .Description("see reports")
                .Do(async (e) =>
                {
                    string res = string.Empty;
                    foreach(KeyValuePair<string,int> kv in reports)
                    {
                        res += "User " + kv.Key + ", reported " + kv.Value +" times!\n";
                    }

                    await e.Channel.SendMessage(res);
                });
        }

        private string listRexDB(string username)
        {
            string res = string.Empty;
            res += "```Markdown\n";
            foreach(string s in rexDB[username].Keys)
            {
                res += "id: <" + s + "> contents: <" + rexDB[username][s] + ">\n";
            }
            res += "```";
            return res;
        }


        private void loadRexDB()
        {
            //Usernameㄱidㄱcontent
            string line;

            try
            {
                using (StreamReader sr = new StreamReader(textPath + "rexdb.txt"))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] afterSplit = line.Split('ㄱ');
                        string userName = afterSplit[0];
                        string id = afterSplit[1];
                        string content = afterSplit[2];
                        if (!rexDB.ContainsKey(userName))
                        {
                            rexDB[userName] = new Dictionary<string, string>();
                        }
                        if (rexDB[userName].ContainsKey(id))
                        {
                            rexDB[userName][id] = content;
                        }
                        else
                        {
                            rexDB[userName].Add(id, content);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("rexDB load error");
                Console.WriteLine(e.Message);
            }
        }

        //return content from dictionary
        private string getFromRexDB(string username,string id)
        {
            string res = string.Empty;
            if (rexDB.ContainsKey(username) && rexDB[username].ContainsKey(id))
            {
                res = rexDB[username][id];
            } else
            {
                res = "You have nothing stored here";
            }

            return res;
        }

        //Add line to file + update current dictionary
        //File wont delete existing id's
        //Load process will simply overwrite so most recent pair survives
        private void writeToRexDB(string userName,string id, string content)
        {
            if (ContainsAny(content, new string[] { "!meme" }) && content.Count(x => x == '(') == 3)
            {

                int bracketCount = 0;
                string type = string.Empty;
                string topText = string.Empty;
                string botText = string.Empty;

                for (int i = 0; i < content.Length; i++)
                {

                    if (content[i] == '(' || content[i] == ')')
                    {
                        bracketCount++;
                    }
                    if (bracketCount == 3)
                    {
                        if (content[i] != '(')
                            topText += content[i];
                    }
                    if (bracketCount == 5)
                    {
                        if (content[i] != '(')
                            botText += content[i];
                    }
                    if (bracketCount == 1)
                    {
                        if (content[i] != ')' && content[i] != '(')
                            type += content[i];
                    }
                }
                topText = topText.Replace(' ', '-');
                botText = botText.Replace(' ', '-');
                Console.WriteLine(topText);
                Console.WriteLine(botText);
                string final = "https://memegen.link/" + type + "/" + topText + "/" + botText + ".jpg";
                content = final;
            }


            if (!rexDB.ContainsKey(userName))
            {
                rexDB[userName] = new Dictionary<string, string>();
            }
            
            if (rexDB[userName].ContainsKey(id))
            {
                rexDB[userName][id] = content;
            } else
            {
                rexDB[userName].Add(id, content);
            }

            using (StreamWriter sw = File.AppendText("texts/rexdb.txt"))
            {
                sw.WriteLine(userName + 'ㄱ' + id + 'ㄱ' + content);
            }



        }

        private bool isMode(string reqMode)
        {
            string check = reqMode.ToLower();
            foreach(KeyValuePair<string, RexMode> kv in modes)
            {
                if (reqMode==kv.Key)
                {
                    return true;
                }
            }
            return false;
        }

        private string commentOnPic()
        {
            string res = getWord(laughList) + " " + getWord(introList) + " ";

            res += sillyName();
            if (roll(50)) { res += "! " + getWord(expList); }
            return res;
        }

        private string sillyName()
        {
            Random rnzd = new Random();
            int rz = rnzd.Next(4);
            string res = string.Empty;
            for (int i = 0; i < rz; i++)
            {
                res += getWord(adjList) + " ";
            }

            res += getWord(nounList);
            return res;
        }

        private Boolean roll(int chance)
        {
            int r = rnd.Next(0,100);
            if(r<=chance)
            {
                return true;
            } else
            {
                return false;
            }
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private string getWord(List<string> l)
        {
            int r = rnd.Next(l.Count);
            return (string)l[r];
        }

        private void populateResponses()
        {
            string line;
            try
            {
                using (StreamReader sr = new StreamReader(textPath + "responses.txt"))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        int count = line.Count(x => x == '|');
                        count++;
                        string[] res = line.Split('ㄱ');
                        string[] resSplit = res[1].Split('|');
                        string[] responseStrings = new string[count];


                        for(int i=0; i<count; i++)
                        {
                            responseStrings[i] = resSplit[i];
                        }

                        responses[res[0]] = responseStrings;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("populate responses error");
                Console.WriteLine(e.Message);
            }
        }

        private void populate(List<string> l, string filename)
        {
            string line;
            try
            {
                using (StreamReader sr = new StreamReader(textPath + filename))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        l.Add(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("populate error");
                Console.WriteLine(e.Message);
            }
        }

        private string getAliases()
        {
            string res = string.Empty;
            res += "Aliases...\n";
            foreach (KeyValuePair<string[], string> entry in aliases)
            {
                res += entry.Value.ToString() + " : ";
                for(int i=0; i<entry.Key.Length; i++)
                {
                    res +=  '`'+ entry.Key[i].ToString()+'`';
                    if(i< entry.Key.Length - 1)
                    {
                        res += ", ";
                    }
                }
                res += "\n";
            }
            res += "";
            if (res.Length > 2000)
            {
                res = "Too many aliases to display at once";
            }
            return res;
        }

        private void ParseAliases()
        {
            string line;

            string res=string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(textPath + "alias.txt"))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] initSplit = line.Split('\t');
                        string name = initSplit[0];
                        string[] aliasesString = new string[initSplit.Length-1];
                        for(int i=0; i<initSplit.Length-1; i++)
                        {
                            aliasesString[i] = initSplit[i + 1];
                        }
                        aliases.Add(aliasesString, name);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("alias parse error");
                Console.WriteLine(e.Message);
            }


            //using (StreamWriter outputFile = new StreamWriter(textPath + "aliasProcessed.txt"))
            //{
            //    foreach (string s in lines)
            //        outputFile.WriteLine(s.Replace("\t", ", "));
            //}



        }
        private string getAllModesInfo()
        {
            string res = string.Empty;

            foreach(KeyValuePair<string, RexMode> kv in modes)
            {
                res += getModeInfo(kv.Value) + "\n";
            }

            return res;
        }

        private string getModeInfo(RexMode rm)
        {
            string info = string.Empty;

            info += "**" + rm.getName() + "** - " + rm.getDescription() + " { ";
            foreach(string perm in rm.getPermissions())
            {
                info += perm + " ";
            }
            info += "}";

            return info;
        }

        private void populatePicFileNames()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\Rexyrex\Documents\visual studio 2015\Projects\testBot\testBot\bin\Debug\friendpics");
            FileInfo[] Files = d.GetFiles("*.*");
            string str = "";
            foreach (FileInfo file in Files)
            {
                picNames.Add(file.Name);
            }
            Console.WriteLine(str);
        }

        public string makeHTTPRequest(string givenURL)
        {
            string responseStr = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(givenURL);
            

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException("Error code: " + response.StatusCode);
                }
                using (System.IO.Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            responseStr = reader.ReadToEnd();
                        }
                    }
                }
            }
            return responseStr;
        }

        public string makeHTTPRequest2(string givenURL)
        {
            string responseStr = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(givenURL);
            
            request.Headers["Authorization"] = "Client-ID 1a8e14c14351c3b";
            //fc417f28ef7135ba5f77ea6daef1e231d681ed3c
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException("Error code: " + response.StatusCode);
                }
                using (System.IO.Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            responseStr = reader.ReadToEnd();
                        }
                    }
                }
            }
            return responseStr;
        }
    }
}
