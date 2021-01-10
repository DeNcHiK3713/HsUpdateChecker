using CASCLib;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.IO;
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
            CASCConfig.LoadFlags |= LoadFlags.Install;

            var pcConfig = CASCConfig.LoadOnlineStorageConfig("hsb", "eu");
            var androidConfig = CASCConfig.LoadOnlineStorageConfig("hsb", "android_google");
            bool updateFile = true;
            if (File.Exists(Settings.FilePath))
            {
                var text = File.ReadAllLines(Settings.FilePath);
                bool pc = pcConfig.VersionName != text[0];
                bool android = androidConfig.VersionName != text[1];
                updateFile = pc || android;
                if (updateFile)
                {
                    TelegramBotClient tgClient = new TelegramBotClient(Settings.TgToken);
                    DiscordWebhookClient dsClient = new DiscordWebhookClient(Settings.DsWebhookId, Settings.DsWebhookToken);
                    if (pc)
                    {
                        var message = $"🆕New version released\n⚔️Platform: PC🖥️\n#️⃣Version: {pcConfig.VersionName}\n🆔BuildId: {pcConfig.ActiveBuild}";
                        var tasks = new List<Task>();
                        tasks.Add(tgClient.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(Settings.TgChatId), message));
                        tasks.Add(dsClient.SendMessageAsync(message));

                        var cascHandler = CASCHandler.OpenStorage(pcConfig);
                        cascHandler.Root.SetFlags(LocaleFlags.ruRU, false);
                        cascHandler.Root.MergeInstall(cascHandler.Install);

                        cascHandler.SaveFileTo(Settings.FileName, Settings.SavePath);

                        await Task.WhenAll(tasks);
                    }
                    if (android)
                    {
                        var message = $"🆕New version released\n⚔️Platform: Android📱\n#️⃣Version: {androidConfig.VersionName}\n🆔BuildId: {androidConfig.ActiveBuild}";
                        var tasks = new List<Task>();
                        tasks.Add(tgClient.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(Settings.TgChatId), message));
                        tasks.Add(dsClient.SendMessageAsync(message));
                        await Task.WhenAll(tasks);
                    }
                }
            }
            if (updateFile)
            {
                File.WriteAllText(Settings.FilePath, $"{pcConfig.VersionName}{Environment.NewLine}{androidConfig.VersionName}");
            }
        }
    }
}
