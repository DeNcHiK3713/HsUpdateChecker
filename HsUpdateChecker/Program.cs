using Discord;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HsUpdateChecker
{
    class Program
    {
        private static SettingsManager SettingsManager = new SettingsManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"));
        private static AppSettings Settings = SettingsManager.GetSection<AppSettings>();
        static async Task Main(string[] args)
        {
            var url = string.Format(Settings.Server, "eu", "hsb", "versions");
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            VersionsInfo pcInfo;
            VersionsInfo androidInfo;
            using (var stream = response.GetResponseStream())
            {
                var info = VersionsHelper.Parse(stream);
                pcInfo = info.First(x => x.Region == "eu");
                androidInfo = info.First(x => x.Region == "android_google");
            }
            response.Close();
            bool updateFile = true;
            if (File.Exists(Settings.FilePath))
            {
                var text = File.ReadAllLines(Settings.FilePath);
                bool pc = pcInfo.VersionsName != text[0];
                bool android = androidInfo.VersionsName != text[1];
                updateFile = pc || android;
                if (updateFile)
                {
                    TelegramBotClient tgClient = new TelegramBotClient(Settings.TgToken);
                    DiscordWebhookClient dsClient = new DiscordWebhookClient(Settings.DsWebhookId, Settings.DsWebhookToken);
                    if (pc)
                    {
                        var message = $"🆕New version released\n⚔️Platform: PC🖥️\n#️⃣Version: {pcInfo.VersionsName}\n🆔BuildId: {pcInfo.BuildId}";
                        var tasks = new List<Task>();
                        tasks.Add(tgClient.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(Settings.TgChatId), message));
                        tasks.Add(dsClient.SendMessageAsync(message));
                        await Task.WhenAll(tasks);
                    }
                    if (android)
                    {
                        var message = $"🆕New version released\n⚔️Platform: Android📱\n#️⃣Version: {androidInfo.VersionsName}\n🆔BuildId: {androidInfo.BuildId}";
                        var tasks = new List<Task>();
                        tasks.Add(tgClient.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(Settings.TgChatId), message));
                        tasks.Add(dsClient.SendMessageAsync(message));
                        await Task.WhenAll(tasks);
                    }
                }
            }
            if (updateFile)
            {
                File.WriteAllText(Settings.FilePath, $"{pcInfo.VersionsName}{Environment.NewLine}{androidInfo.VersionsName}");
            }
        }
    }
}
