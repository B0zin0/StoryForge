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
        private static DiscordRpcClient? client;

        public static int Initialize(string clientId)
        {
            Dispose();
            try
            {
                client = new DiscordRpcClient(clientId);
                client.OnReady += (sender, e) =>
                {
                    Debug.WriteLine($"[StoryForge] >> Connected to Discord as {e.User.Username}");
                };
                client.Initialize();
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StoryForge] >> Failed to initialize Discord RPC: {ex.Message}");
                return -1;
            }
        }

        public static void SetPresence(string details, string state, string imageKey = "storyforge_logo", string imageText = "StoryForge Launcher")
        {
            if (client == null)
            {
                Debug.WriteLine("[StoryForge] >> Discord RPC client is not initialized.");
                return;
            }

            try
            {
                client.SetPresence(new RichPresence()
                {
                    Details = details,
                    State = state,
                    Assets = new Assets()
                    {
                        LargeImageKey = imageKey,
                        LargeImageText = imageText,
                        SmallImageKey = imageKey,
                        SmallImageText = imageText
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StoryForge] >> Failed to set Discord presence: {ex.Message}");
            }
        }
        
        public static void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }
    }
}
