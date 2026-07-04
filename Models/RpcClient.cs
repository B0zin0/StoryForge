using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryForge.Models
{
    public static class RpcClient
    {
        private static DiscordRpcClient? _client;

        public static DiscordRpcClient? Client { get => _client; }

        public static int Initialize(string clientId)
        {
            Dispose();
            try
            {
                _client = new DiscordRpcClient(clientId);
                _client.OnReady += (sender, e) =>
                {
                    Debug.WriteLine($"[StoryForge] >> Connected to Discord as {e.User.Username}");
                };
                _client.Initialize();
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StoryForge] >> Failed to initialize Discord RPC: {ex.Message}");
                return -1;
            }
        }

        public static void SetPresence(
            string details, string state,
            string largeImageKey = "story_icon", string largeImageText = "StoryForge Launcher",
            string? smallImageKey = null, string? smallImageText = null)
        {
            if (_client == null)
            {
                Debug.WriteLine("[StoryForge] >> Discord RPC client is not initialized.");
                return;
            }

            try
            {
                _client.SetPresence(new RichPresence()
                    {
                        Details = details,
                        State = state,
                        Assets = new Assets()
                        {
                            LargeImageKey = largeImageKey,
                            LargeImageText = largeImageText,
                            SmallImageKey = smallImageKey ?? largeImageKey,
                            SmallImageText = smallImageText ?? largeImageText
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StoryForge] >> Failed to set Discord presence: {ex.Message}");
            }
        }
        
        public static void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }
    }
}
