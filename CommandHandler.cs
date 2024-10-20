using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Miya_
{
    public class CommandHandler : DiscordClientService 
    {
        private readonly IServiceProvider provider;
        public DiscordSocketClient client;
        private readonly CommandService service;
        private readonly IConfiguration configuration;
        private static string aiKey;
        private AI_Miya AI;
        private IUser recUser;
        
        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration configuration, ILogger<CommandHandler> logger) : base (client, logger)
        {
            this.provider = provider;
            this.client = client;
            this.service = service;
            this.configuration = configuration;

            AI = new AI_Miya(configuration["OpenAI-Key"]);
            recUser = client.GetUser(937862407336898580); // (ME)
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.client.MessageReceived += OnMessageReceived;
            await this.service.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
        }
        static Random random = new Random();
        private async Task OnMessageReceived(SocketMessage socketMessage) 
        {
            if (!(socketMessage is SocketUserMessage message)) return;
            if (message.Source != Discord.MessageSource.User) return; 
            var argPos = 0;
            var context = new MMSocketCommandContext(this.client, message, AI.GetMsgList());
            context.AddRecUser(recUser);

            //UNCOMMENT TO USE MODMAIL

            //if (message.Channel is IDMChannel && !message.HasStringPrefix(this.configuration["Prefix"], ref argPos))
            //{
            //    IUser user = context.Message.Author;
            //
            //    ITextChannel Modchannel = context.Client.GetChannel(371002345754198028) as ITextChannel;
            //
            //    if (recUser != user)
            //    {
            //        recUser = user;
            //        context.AddRecUser(recUser);
            //        await Modchannel.SendMessageAsync("**New user DM**\n`Username:` " + recUser.Username);
            //        await Modchannel.SendMessageAsync("`ID:` " + recUser.Id);
            //    }
            //    List<string> links = new List<string>();
            //
            //    foreach (var x in context.Message.Attachments)
            //    {
            //        links.Add(x.Url);
            //    }
            //
            //    await message.AddReactionAsync(new Emoji("\U0001F4E8"));
            //    var EmbedBuilder = new EmbedBuilder();
            //    EmbedBuilder.AddField("ModMail:", $"{user.Mention} - {message}")
            //
            //    .WithAuthor(user)
            //    .WithColor(Color.Blue)
            //    .WithFooter(footer =>
            //    {
            //        footer
            //        .WithText($"\n{context.User.Id}");
            //    }
            //    )
            //    .WithCurrentTimestamp();
            //
            //    if (links.Count > 0) { EmbedBuilder.WithDescription("Attachments below:"); }
            //
            //    Embed embed = EmbedBuilder.Build();
            //
            //    await Modchannel.SendMessageAsync(embed: embed);
            //
            //    string Attachments = null;
            //    string Attachments2 = null;
            //
            //    if (links.Count > 0 && links.Count < 5)
            //    {
            //        foreach (var z in links)
            //        {
            //            Attachments += z + Environment.NewLine;
            //        }
            //        await Modchannel.SendMessageAsync(Attachments);
            //    }
            //    else if (links.Count > 5)
            //    {
            //        for (int i = 0; i < 5; i++)
            //        {
            //            Attachments += links[i] + Environment.NewLine;
            //        }
            //        for (int o = 0; o < links.Count - 5; o++)
            //        {
            //            Attachments2 += links[o + 5] + Environment.NewLine;
            //        }
            //    }
            //
            //}
            bool isReplyToMiya = false;
            if (message.Reference != null) { isReplyToMiya = message.ReferencedMessage.Author.Id == this.client.CurrentUser.Id; }

            if (message.Channel is IDMChannel && !message.HasStringPrefix(this.configuration["Prefix"], ref argPos) && message.Author != context.Client.GetUser(218445513547186176))
            {
                IUser user = context.Message.Author;

                IUser ME = context.Client.GetUser(218445513547186176);

                List<string> links = new List<string>();

                foreach (var x in context.Message.Attachments)
                {
                    links.Add(x.Url);
                }

                await message.AddReactionAsync(new Emoji("\U0001F4E8"));
                var EmbedBuilder = new EmbedBuilder();
                EmbedBuilder.AddField("Miya:", $"{user.Mention} - {message}")

                .WithAuthor(user)
                .WithColor(Color.Magenta)
                .WithFooter(footer =>
                {
                    footer
                    .WithText($"\n{context.User.Id}");
                }
                )
                .WithCurrentTimestamp();

                if (links.Count > 0) { EmbedBuilder.WithDescription("Attachments below:"); }

                Embed embed = EmbedBuilder.Build();

                await ME.SendMessageAsync(embed: embed);

                string Attachments = null;
                string Attachments2 = null;

                if (links.Count > 0 && links.Count < 5)
                {
                    foreach (var z in links)
                    {
                        Attachments += z + Environment.NewLine;
                    }
                    await ME.SendMessageAsync(Attachments);
                }
                else if (links.Count > 5)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Attachments += links[i] + Environment.NewLine;
                    }
                    for (int o = 0; o < links.Count - 5; o++)
                    {
                        Attachments2 += links[o + 5] + Environment.NewLine;
                    }
                }
                return;
            }
            else if (message.Channel is IDMChannel && message.Author == context.Client.GetUser(218445513547186176))
            {
                string responce = await AI.ChatAI(message);
                
                await message.ReplyAsync(responce);
                if (responce == null) { return; }

                Console.OutputEncoding = System.Text.Encoding.Unicode;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"AImsg: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"At -> [{message.Author.Username}'s DMs]");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("Prmpt: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(message.ToString());

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"AIMsg: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(responce + "\n");
                return;
            }
            else if (message.HasMentionPrefix(this.client.CurrentUser, ref argPos) || (random.Next(0,9999) == 99) || isReplyToMiya)
            {
                string responce = await AI.ChatAI(message);
                
                await message.ReplyAsync(responce);
                if (responce == null) { return; }

                Console.OutputEncoding = System.Text.Encoding.Unicode;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("AiLg: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"FROM [{message.Author.Username}] At -> [{context.Guild.Name}] : [{message.Channel.Name}]");

                string msgString;

                int index = message.ToString().IndexOf("<@911363108675682394>");
                if (index >= 0)
                {
                    msgString =  message.ToString().Substring(0, index) + message.ToString().Substring(index + "<@911363108675682394>".Length);
                }
                else { msgString = message.ToString(); }


                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("User: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(message.ToString().Substring(msgString.IndexOf(">") + 1).TrimStart());

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"Miya: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(responce + "\n");
                return;
            }
            else if (!message.HasStringPrefix(this.configuration["Prefix"], ref argPos)) return;
            await this.service.ExecuteAsync(context, argPos, this.provider);

            var channel = message.Channel as IGuildChannel;
        }
    }
    public class MMSocketCommandContext : ICommandContext
    {
        public DiscordSocketClient Client { get; }
        public SocketGuild Guild { get; }
        public ISocketMessageChannel Channel { get; }
        public SocketUser User { get; }
        public SocketUserMessage Message { get; }
        public bool IsPrivate => Channel is IPrivateChannel;
        public IUser recUser;
        public List<dynamic> msgList = new List<dynamic>();
        public void AddRecUser(IUser recU)
        {
            recUser = recU;
        }
        IDiscordClient ICommandContext.Client => Client;

        IGuild ICommandContext.Guild => Guild;

        IMessageChannel ICommandContext.Channel => Channel;

        IUser ICommandContext.User => User;

        IUserMessage ICommandContext.Message => Message;
        public MMSocketCommandContext(DiscordSocketClient client, SocketUserMessage msg, List<dynamic> msgL)
        {
            Client = client;
            Guild = (msg.Channel as SocketGuildChannel)?.Guild;
            Channel = msg.Channel;
            User = msg.Author;
            Message = msg;
            msgList = msgL;
        }
    }
    public class AI_Miya
    {
        private string key;
        private const string url = "https://api.openai.com/v1/chat/completions";
		string systemPrompt = "Sample system prompt"
        public AI_Miya(string key)
        {
            this.key = key;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
        }
        public static List<dynamic> msgList = new List<dynamic>
        {
            new { role    = "system",
                  content = systemPrompt}
        };
        private static bool LimitReached = false;
        public static Task ResetLogs()
        {
            msgList = new List<dynamic>
            {
                new { role    = "system",
                  content = systemPrompt}
            };
            return Task.CompletedTask;
        }

        private HttpClient client = new HttpClient();
        public static void MsgListAdd(string input)
        {
            msgList.Add(new { role = "user", content = input });
        }
        
        public List<dynamic> GetMsgList()
        {
            return msgList;
        }
        public async Task removeItems()
        {
            Console.WriteLine("!!! Items removed !!!");
            await Task.Run(() => msgList.RemoveRange(1, 2));
        }
        public async Task<string> ChatAI(SocketUserMessage message)
        {
            if (LimitReached) { await removeItems(); LimitReached = false; }
            var messages = new List<dynamic>();
            string userMessage;
            
            if (message.ToString().Contains("<@911363108675682394>")) 
            {
                userMessage = $"{message.Author.Username}| " + message.ToString().Substring(message.ToString().IndexOf('>')+1).TrimStart(); 
            }
            else { userMessage = $"{message.Author.Username}| " + message.ToString(); }
            
            messages = msgList.ToList();
            messages.Add(new { role = "user", content = userMessage });

            var request = new
            {
                messages, 
                model = "gpt-3.5-turbo-0125", 
                max_tokens = 200, 
                n = 1
            };

            var requestJson = JsonConvert.SerializeObject(request);
            var httpResponseMessage = await client.PostAsync(url, new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json"));
            var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
            {
                choices = new[] { new { message = new { role = string.Empty, content = string.Empty } } },
                error = new { message = string.Empty }
            });

            if (!string.IsNullOrEmpty(responseObject?.error?.message))
            {
                Console.WriteLine();
                ConsoleColor temp = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Error: " + responseObject?.error?.message);
                Console.ForegroundColor = temp;
                Console.WriteLine();
                if (responseObject.error.message.Contains("This model's maximum context length is 4097 tokens."))
                {
                    LimitReached = true;
                    await removeItems();

                    messages = new List<dynamic>();
                    messages = msgList.ToList();
                    
                    return await ChatAI(message);
                }

                msgList = messages.ToList();
                return null;
            }
            else  
            {
                msgList = messages.ToList();
                var messageObject = responseObject?.choices[0].message; 
                msgList.Add(messageObject); // Add the message object to the message collection
                return messageObject.content;
            }
        }
    }
}
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;
//using System.Reflection;
//using Discord;
//using Discord.Commands;
//using Discord.Addons.Hosting;
//using Discord.WebSocket;
//using System.Collections.Generic;
//using Discord.Rest;

//namespace Miya_
//{
//    public class CommandHandler : InitializedService
//    {
//        private readonly IServiceProvider provider;
//        public DiscordSocketClient client;
//        private readonly CommandService service;
//        private readonly IConfiguration configuration;
//        private IUser recUser;

//        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration configuration)
//        {
//            this.provider = provider;
//            this.client = client;
//            this.service = service;
//            this.configuration = configuration;
//            recUser = client.GetUser(937862407336898580);
//        }

//        public override async Task InitializeAsync(CancellationToken cancellationToken)
//        {
//            this.client.MessageReceived += OnMessageReceived;
//            await this.service.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
//        }

//        private async Task OnMessageReceived(SocketMessage socketMessage)
//        {
//            if (!(socketMessage is SocketUserMessage message)) return;
//            if (message.Source != Discord.MessageSource.User) return;

//            var argPos = 0;
//            var context = new MMSocketCommandContext(this.client, message);
//            context.AddRecUser(recUser);

//            if (message.Channel is IDMChannel && !message.HasStringPrefix(this.configuration["Prefix"], ref argPos))
//            {
//                IUser user = context.Message.Author;

//                ITextChannel Modchannel = context.Client.GetChannel(371002345754198028) as ITextChannel;

//                if (recUser != user)
//                {
//                    recUser = user;
//                    context.AddRecUser(recUser);
//                    await Modchannel.SendMessageAsync("**New user DM**\n`Username:` " + recUser.Username);
//                    await Modchannel.SendMessageAsync("`ID:` " + recUser.Id);
//                }
//                List<string> links = new List<string>();

//                foreach (var x in context.Message.Attachments)
//                {
//                    links.Add(x.Url);
//                }

//                await message.AddReactionAsync(new Emoji("\U0001F4E8"));
//                var EmbedBuilder = new EmbedBuilder();
//                EmbedBuilder.AddField("ModMail:", $"{user.Mention} - {message}")

//                .WithAuthor(user)
//                .WithColor(Color.Blue)
//                .WithFooter(footer =>
//                {
//                    footer
//                    .WithText($"\n{context.User.Id}");
//                }
//                )
//                .WithCurrentTimestamp();

//                if (links.Count > 0) { EmbedBuilder.WithDescription("Attachments below:"); }

//                Embed embed = EmbedBuilder.Build();

//                await Modchannel.SendMessageAsync(embed: embed);

//                string Attachments = null;
//                string Attachments2 = null;

//                if (links.Count > 0 && links.Count < 5)
//                {
//                    foreach (var z in links)
//                    {
//                        Attachments += z + Environment.NewLine;
//                    }
//                    await Modchannel.SendMessageAsync(Attachments);
//                }
//                else if (links.Count > 5)
//                {
//                    for (int i = 0; i < 5; i++)
//                    {
//                        Attachments += links[i] + Environment.NewLine;
//                    }
//                    for (int o = 0; o < links.Count - 5; o++)
//                    {
//                        Attachments2 += links[o + 5] + Environment.NewLine;
//                    }
//                }

//            }
//            else if (!message.HasStringPrefix(this.configuration["Prefix"], ref argPos) && !message.HasMentionPrefix(this.client.CurrentUser, ref argPos)) return;

//            await this.service.ExecuteAsync(context, argPos, this.provider);

//            var channel = message.Channel as IGuildChannel;
//        }
//    }
//    public class MMSocketCommandContext : ICommandContext
//    {
//        public DiscordSocketClient Client { get; }
//        public SocketGuild Guild { get; }
//        public ISocketMessageChannel Channel { get; }
//        public SocketUser User { get; }
//        public SocketUserMessage Message { get; }
//        public bool IsPrivate => Channel is IPrivateChannel;
//        public IUser recUser;
//        public void AddRecUser(IUser recU)
//        {
//            recUser = recU;
//        }
//        IDiscordClient ICommandContext.Client => Client;

//        IGuild ICommandContext.Guild => Guild;

//        IMessageChannel ICommandContext.Channel => Channel;

//        IUser ICommandContext.User => User;

//        IUserMessage ICommandContext.Message => Message;
//        public MMSocketCommandContext(DiscordSocketClient client, SocketUserMessage msg)
//        {
//            Client = client;
//            Guild = (msg.Channel as SocketGuildChannel)?.Guild;
//            Channel = msg.Channel;
//            User = msg.Author;
//            Message = msg;
//        }
//    }
//}
