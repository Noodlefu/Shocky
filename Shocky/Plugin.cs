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

namespace Shocky
{
    public sealed class ShockyPlugin : IDalamudPlugin
    {
        public static string Name => "Shocky";
        public static ShockyPlugin? PluginInstance { get; private set; }
        public Configuration Configuration { get; init; }
        private readonly WindowSystem windowSystem = new("Shocky");
        private ConfigWindow ConfigWindow { get; init; }

        public ShockyPlugin(DalamudPluginInterface pluginInterface)
        {
            DalamudApi.Initialize(this, pluginInterface);
            PluginInstance = this;
            Configuration = DalamudApi.PluginInterface?.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(pluginInterface);

            ConfigWindow = new ConfigWindow(this);

            windowSystem.AddWindow(ConfigWindow);

            DalamudApi.PluginInterface!.UiBuilder.Draw += DrawUI;
            DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            DalamudApi.ChatGui!.ChatMessage += Chat_OnChatMessage;
        }

        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!Configuration.ChatListeners.Contains(type))
                return;

            var messageText = message.TextValue;
            var foundTrigger = Configuration.Triggers.FirstOrDefault(trigger => trigger.TriggerWord == messageText);

            if (foundTrigger == null || foundTrigger.TriggerWord.Length == 0)
                return;

            var shockRequest = new ShockRequest
            {
                Username = Configuration.Username,
                Name = "FFXIV Shocky",
                Code = Configuration.Code,
                Intensity = foundTrigger.Intensity,
                Duration = foundTrigger.Duration,
                ApiKey = Configuration.ApiKey,
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
                DalamudApi.Log?.Debug($"HTTP request code: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                DalamudApi.Log?.Debug($"HTTP request failed: {ex.Message}");
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
