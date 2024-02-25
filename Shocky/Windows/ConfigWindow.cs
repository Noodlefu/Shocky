using System;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Newtonsoft.Json;

namespace Shocky.Windows
{
    public class ConfigWindow : Window, IDisposable
    {
        private readonly Configuration configuration;

        private static DeviceInfo? DeviceInfo;

        public ConfigWindow(ShockyPlugin plugin) : base("Config window", ImGuiWindowFlags.NoCollapse)
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(700, 400),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            configuration = plugin.Configuration;
            GetDeviceData();
        }

        public void Dispose() => GC.SuppressFinalize(this);

        private void GetDeviceData()
        {
            if (configuration.ApiKey.Length > 0 && configuration.Username.Length > 0 && configuration.Code.Length > 0)
            {
                var shockRequest = new ShockRequest
                {
                    Username = configuration.Username,
                    Name = "FFXIV Shocky",
                    Code = configuration.Code,
                    ApiKey = configuration.ApiKey,
                };

                var jsonContent = JsonConvert.SerializeObject(shockRequest);

                _ = PostJsonData(jsonContent);
            }
        }

        public override void Draw()
        {
            DrawApiInfo();
            DrawDeviceInfo();
            DrawTriggerListeningChannels();
            DrawTriggers();

            if (ImGui.Button("Save"))
            {
                configuration.Save();
            }
        }

        private void DrawApiInfo()
        {
            if (ImGui.CollapsingHeader("Api Info"))
            {
                ImGui.Indent(30);
                var apiKey = configuration.ApiKey;
                if (ImGui.InputText("ApiKey", ref apiKey, 255))
                {
                    configuration.ApiKey = apiKey;
                    GetDeviceData();
                }
                var username = configuration.Username;
                if (ImGui.InputText("Username", ref username, 255))
                {
                    configuration.Username = username;
                    GetDeviceData();
                }
                var shockCode = configuration.Code;
                if (ImGui.InputText("Code", ref shockCode, 255))
                {
                    configuration.Code = shockCode;
                    GetDeviceData();
                }
                ImGui.Unindent(30);
            }
        }

        private void DrawDeviceInfo()
        {
            if (DeviceInfo != null && ImGui.CollapsingHeader("Device Info"))
            {
                ImGui.Indent(30);
                var error = DeviceInfo.error;
                if (error.Length > 0)
                {
                    ImGui.LabelText("Error", error.ToString());
                }
                else
                {
                    ImGui.LabelText(DeviceInfo.clientId.ToString(), "Client ID");
                    ImGui.LabelText(DeviceInfo.id.ToString(), "ID");
                    ImGui.LabelText(DeviceInfo.name.ToString(), "Name");
                    ImGui.LabelText(DeviceInfo.paused.ToString(), "Paused?");
                    ImGui.LabelText(DeviceInfo.maxIntensity.ToString(), "Max Intensity");
                    ImGui.LabelText(DeviceInfo.maxDuration.ToString(), "Max Duration");
                    ImGui.LabelText(DeviceInfo.online.ToString(), "Online?");
                }
                ImGui.Unindent(30);
            }
        }

        private void DrawTriggerListeningChannels()
        {
            if (ImGui.CollapsingHeader("Trigger Listening Channels"))
            {
                ImGui.Indent(30);

                foreach (var chatEntry in (XivChatType[])Enum.GetValues(typeof(XivChatType)))
                {
                    if (IsExcludedChatType(chatEntry)) continue;

                    var configValue = configuration.ChatListeners.Contains(chatEntry);
                    if (ImGui.Checkbox(chatEntry.ToString(), ref configValue))
                    {
                        ToggleChatListener(chatEntry, configValue);
                    }
                }
                ImGui.Unindent(30);
            }
        }

        private void DrawTriggers()
        {
            if (ImGui.CollapsingHeader("Triggers"))
            {
                foreach (var trigger in configuration.Triggers)
                {
                    DrawTrigger(trigger);
                }

                if (ImGui.Button("Add Trigger"))
                {
                    configuration.Triggers.Add(new Trigger());
                }
            }
        }

        private void DrawTrigger(Trigger trigger)
        {
            ImGui.BeginGroup();
            ImGui.Indent(30);
            //DrawRemoveButton(trigger);
            var triggerWord = trigger.TriggerWord;
            if (ImGui.InputText($"##Word{trigger.Id}", ref triggerWord, 100))
            {
                trigger.TriggerWord = triggerWord;
            }

            ImGui.SameLine();
            if (ImGui.Button($"X##Rem{trigger.Id}"))
            {
                configuration.Triggers.Remove(trigger);
            }

            ImGui.Text("Operation Type");
            ImGui.SameLine();
            var operationType = trigger.OperationType;
            if (ImGui.InputInt($"##OpType{trigger.Id}", ref operationType))
            {
                trigger.OperationType = operationType;
            }

            ImGui.Text("Duration");
            ImGui.SameLine();
            var duration = trigger.Duration;
            if (ImGui.InputInt($"##Dur{trigger.Id}", ref duration))
            {
                trigger.Duration = duration;
            }

            ImGui.Text("Intensity");
            ImGui.SameLine();
            var intensity = trigger.Intensity;
            if (ImGui.InputInt($"##Int{trigger.Id}", ref intensity))
            {
                trigger.Intensity = intensity;
            }
            ImGui.Unindent(30);
            ImGui.EndGroup();

            ImGui.Separator();
        }

        private static async Task<DeviceInfo> PostJsonData(string jsonContent)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://do.pishock.com/api/GetShockerInfo/")
            };

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    DeviceInfo = JsonConvert.DeserializeObject<DeviceInfo>(jsonResponse);
                }
                else
                {
                    HandleHttpRequestFailure(response.StatusCode.ToString());
                }
            }
            catch (HttpRequestException ex)
            {
                HandleHttpRequestFailure(ex.Message);
            }
            return new DeviceInfo();
        }

        private static void HandleHttpRequestFailure(string errorMessage)
        {
            DalamudApi.Log?.Debug($"HTTP request failed: {errorMessage}");
            DeviceInfo = new DeviceInfo { error = $"HTTP request failed: {errorMessage}" };
        }

        private static void DrawTextInput(string label, ref string value, Action? onChange = null)
        {
            if (ImGui.InputText(label, ref value, 255) && onChange != null)
            {
                onChange.Invoke();
            }
        }

        private static void DrawTextInput(string label, ref int value)
        {
            ImGui.Text(label);
            ImGui.SameLine();
            ImGui.InputInt(label, ref value);
        }

        private void DrawRemoveButton(Trigger trigger)
        {
            ImGui.SameLine();
            if (ImGui.Button($"X##Rem{trigger.Id}"))
            {
                configuration.Triggers.Remove(trigger);
            }
        }

        private static bool IsExcludedChatType(XivChatType chatEntry)
        {
            return chatEntry == XivChatType.None ||
                   chatEntry == XivChatType.Debug ||
                   chatEntry == XivChatType.Urgent ||
                   chatEntry == XivChatType.Notice ||
                   chatEntry == XivChatType.TellOutgoing ||
                   chatEntry == XivChatType.Echo ||
                   chatEntry == XivChatType.ErrorMessage ||
                   chatEntry == XivChatType.GatheringSystemMessage ||
                   chatEntry == XivChatType.Notice ||
                   chatEntry == XivChatType.NoviceNetwork ||
                   chatEntry == XivChatType.CustomEmote ||
                   chatEntry == XivChatType.SystemMessage ||
                   chatEntry == XivChatType.SystemError ||
                   chatEntry == XivChatType.NPCDialogue ||
                   chatEntry == XivChatType.NPCDialogueAnnouncements ||
                   chatEntry == XivChatType.RetainerSale;
        }

        private void ToggleChatListener(XivChatType chatEntry, bool configValue)
        {
            if (configValue)
            {
                configuration.ChatListeners.Add(chatEntry);
            }
            else
            {
                configuration.ChatListeners.Remove(chatEntry);
            }
        }
    }
}
