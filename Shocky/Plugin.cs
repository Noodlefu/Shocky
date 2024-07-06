using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Shocky.Windows;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Shocky.Classes;

namespace Shocky
{
    public sealed class Shocky : IDalamudPlugin
    {
        public static Shocky? Plugin { get; private set; }
        public Configuration Config { get; init; }
        private readonly WindowSystem windowSystem = new("Shocky");
        private ConfigWindow ConfigWindow { get; init; }

        public Shocky(IDalamudPluginInterface pluginInterface)
        {
            Config = DalamudApi.PluginInterface?.GetPluginConfig() as Configuration ?? new Configuration();
            ConfigWindow = new ConfigWindow(this);
            InitializePlugin(pluginInterface);
            SetupEventHandlers();
        }

        private void InitializePlugin(IDalamudPluginInterface pluginInterface)
        {
            DalamudApi.Initialize(this, pluginInterface);
            Plugin = this;
            Config.Initialize(pluginInterface);
            windowSystem.AddWindow(ConfigWindow);
        }

        private void SetupEventHandlers()
        {
            DalamudApi.PluginInterface!.UiBuilder.Draw += DrawUI;
            DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            DalamudApi.ChatGui!.ChatMessage += Chat_OnChatMessage;
        }

        private void Chat_OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (Config.Triggers.Count == 0 || Config.ChatListeners.Count == 0)
                return;
            if (!Config.ChatListeners.Contains(type))
                return;

            var messageText = message.TextValue;
            var foundTrigger = Config.Triggers
                .OrderByDescending(trigger => trigger.Phrase.Length)
                .FirstOrDefault(trigger => messageText.Contains(trigger.Phrase));

            if (foundTrigger == null || foundTrigger.Phrase.Length == 0)
                return;

            var shockRequest = new PiShockRequest
            {
                Username = Config.Username,
                Name = "FFXIV Shocky",
                Code = Config.Code,
                Intensity = foundTrigger.Intensity,
                Duration = foundTrigger.Duration,
                ApiKey = Config.ApiKey,
                Op = foundTrigger.OperationType
            };

            var jsonContent = JsonConvert.SerializeObject(shockRequest);

            _ = PostJsonData(jsonContent);
        }

        private static async Task PostJsonData(string jsonContent)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://do.pishock.com/api/apioperate/")
            };

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("", content);
                DalamudApi.LogDebug($"HTTP request code: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                DalamudApi.LogDebug($"HTTP request failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            windowSystem.RemoveAllWindows();
            ConfigWindow.Dispose();
        }

        public void DrawUI() => windowSystem.Draw();

        public void DrawConfigUI() => ConfigWindow.IsOpen = true;
    }
}
