using CASCLib;
using Discord.Webhook;
using GooglePlayStoreApi;
using GooglePlayStoreApi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HsUpdateChecker
{
    class Program
    {
        // https://github.com/kagasu/GooglePlayStoreApi/blob/87e4cf9eedd0cbec2e8b7b90959f69edacddf7df/GooglePlayStoreApi/GooglePlayStoreClient.cs#L18
        public static void SetTimeout(TimeSpan timeout)
        {
            var httpClient = (HttpClient)typeof(GooglePlayStoreClient).GetField("client", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            httpClient.Timeout = timeout;
        }

        private static SettingsManager SettingsManager = new SettingsManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"));
        private static AppSettings Settings = SettingsManager.GetSection<AppSettings>();
        static async Task Main(string[] args)
        {
            CASCConfig.LoadFlags |= LoadFlags.Install;

            var pcConfig = CASCConfig.LoadOnlineStorageConfig("hsb", "eu");
            var androidConfig = CASCConfig.LoadOnlineStorageConfig("hsb", "android_google");
            //var androidCnConfig = CASCConfig.LoadOnlineStorageConfig("hsb", "android_cn");
            //var androidCnVersion = Version.Parse(androidCnConfig.VersionName);
            //androidCnVersion = new Version(androidCnVersion.Major, androidCnVersion.Minor, androidCnVersion.Revision);
            //var androidCnVersionString = androidCnVersion.ToString();

            var client = new GooglePlayStoreClient(Settings.Email, Settings.AndroidId, "Android-Finsky/10.4.13-x_86_64 [0] [PR] 198q917767 (api=3,versionCode=80798000,sdk=27,device=angler,hardware=angler,product=angler_x_86_64,platformVersionRelease=8.1.0,model=Nexus 6P,buildId=OPM2.171019.029.B1,isWideScreen=0,supportedAbis=x86_64;x86;arm64-v8a;armeabi-v7a;armeabi)");
            SetTimeout(TimeSpan.FromMinutes(10));
            client.Country = CountryCode.Russia;
            await client.GetGoogleAuth(Settings.GoogleToken);

            var appDetail = await client.AppDetail("com.blizzard.wtcg.hearthstone");
            var versionCode = appDetail.Item.Details.AppDetails.VersionCode;
            var versionString = appDetail.Item.Details.AppDetails.VersionString;
            var offerType = appDetail.Item.Offer[0].OfferType;

            bool updateFile = true;
            if (File.Exists(Settings.FilePath))
            {
                var text = File.ReadAllLines(Settings.FilePath);
                bool pc = pcConfig.VersionName != text[0];
                bool androidCache = androidConfig.VersionName != text[1];
                bool androidApk = versionCode != int.Parse(text[2]);
                //bool androidApkCn = androidCnVersionString != text[3];
                updateFile = pc || androidCache || androidApk /*|| androidApkCn*/;
                if (updateFile)
                {
                    var tgClient = new TelegramBotClient(Settings.TgToken);
                    var dsClient = new DiscordWebhookClient(Settings.DsWebhookId, Settings.DsWebhookToken);
                    if (pc)
                    {
                        var message = $"🆕New version released\n⚔️Platform: PC🖥️\n#️⃣Version: {pcConfig.VersionName}\n🆔BuildId: {pcConfig.BuildId}";
                        var tasks = new List<Task>();
                        tasks.Add(tgClient.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(Settings.TgChatId), message));
                        tasks.Add(dsClient.SendMessageAsync(message));
                        await Task.WhenAll(tasks);
                    }
                    if (androidCache)
                    {
                        var message = $"🆕New version released\n⚔️Platform: Android📱 (cache)\n#️⃣Version: {androidConfig.VersionName}\n🆔BuildId: {androidConfig.BuildId}";
                        var tasks = new List<Task>();
                        tasks.Add(tgClient.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(Settings.TgChatId), message));
                        tasks.Add(dsClient.SendMessageAsync(message));
                        await Task.WhenAll(tasks);
                    }
                    if (androidApk)
                    {
                        var message = $"🆕New version released\n⚔️Platform: Android📱 (apk)\n#️⃣Version: {versionString}\n🆔BuildId: {versionCode}";
                        var tasks = new List<Task>();
                        tasks.Add(tgClient.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(Settings.TgChatId), message));
                        tasks.Add(dsClient.SendMessageAsync(message));

                        await client.Purchase("com.blizzard.wtcg.hearthstone", offerType, versionCode);
                        var bytes = await client.DownloadApk("com.blizzard.wtcg.hearthstone");
                        File.WriteAllBytes(Path.Combine(Settings.SavePath, "Android", $"Hearthstone_{versionString}.apk"), bytes);

                        await Task.WhenAll(tasks);
                    }
                    //if (androidApkCn)
                    //{
                    //    var message = $"🆕New version released\n⚔️Platform: Android📱 (apk CN)\n#️⃣Version: {androidCnVersionString}\n🆔BuildId: {androidCnConfig.BuildId}";
                    //    var tasks = new List<Task>();
                    //    tasks.Add(tgClient.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(Settings.TgChatId), message));
                    //    tasks.Add(dsClient.SendMessageAsync(message));

                    //    var cascHandler = CASCHandler.OpenStorage(androidCnConfig);
                    //    cascHandler.Root.SetFlags(LocaleFlags.zhCN, false);
                    //    cascHandler.Root.MergeInstall(cascHandler.Install);
                    //    var path = Path.Combine(Settings.SavePath, "Android");
                    //    cascHandler.SaveFileTo("apk\\Hearthstone_CN_Production.apk", path);
                    //    File.Move(Path.Combine(path, "Hearthstone_CN_Production.apk"), Path.Combine(path, $"Hearthstone_CN_{androidCnVersionString}.apk"));

                    //    await Task.WhenAll(tasks);
                    //}
                }
            }
            if (updateFile)
            {
                File.WriteAllText(Settings.FilePath, $"{pcConfig.VersionName}{Environment.NewLine}{androidConfig.VersionName}{Environment.NewLine}{versionCode}"); //{Environment.NewLine}{androidCnVersionString}
            }
        }
    }
}
