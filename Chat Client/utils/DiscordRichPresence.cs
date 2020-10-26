using Chat_Client.utils.Log;
using DiscordRPC;
using DiscordRPC.Logging;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Chat_Client.utils
{
    public class DiscordRichPresence
    {
        private const string CLIENT_ID = "764445174188736514";
        private DiscordRpcClient _discordClient;
        private DateTime _start;
        private readonly Gw2Client _api;
        private readonly Dictionary<string, int> _maps;
        private static readonly Logger _log = Logger.getInstance();

        public DiscordRichPresence()
        {
            _api = new Gw2Client();

            //Load the json
            using var stream = Application.GetResourceStream(new Uri("Assets/maps.jsonc", UriKind.Relative)).Stream;
            using var reader = new StreamReader(stream);
            using var maps = new JsonTextReader(reader);
            _maps = JsonConvert.DeserializeObject<Dictionary<string, int>>(JToken.ReadFrom(maps).ToString());
        }
        public void Start()
        {
            _discordClient = new DiscordRpcClient(CLIENT_ID)
            {
                Logger = new ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Warning }
            };

            //Subscribe to events
            _discordClient.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            _discordClient.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received Update! {0}", e.Presence);
            };

            _discordClient.OnError += (sender, e) =>
            {
                _log.AddError(e.Message);
            };

            //Connect to the RPC
            _discordClient.Initialize();
            _start = DateTime.UtcNow;
        }

        public void Stop()
        {
            _discordClient?.Dispose();
            _discordClient = null;
        }

        public void Update(string character, int map_id)
        {
            _api.WebApi.V2.Maps.GetAsync(map_id).ContinueWith(task =>
            {
                if (!task.IsFaulted && task.Result != null)
                {
                    UpdatePresence(character, task.Result);
                }
            });
        }

        private void UpdatePresence(string character, Map map_info) { 
            
            _discordClient.SetPresence(new RichPresence()
            {
                Details = character, //Character Name
                State = $"in {map_info.Name}", //Map Name
                Timestamps = new Timestamps()
                {
                    Start = _start
                },
                Assets = new Assets()
                {
                    LargeImageKey = $"map_{GetMapIcon(map_info)}",
                    LargeImageText = map_info.Name
                }
            });
        }

        private int GetMapIcon(Map map)
        {
            //if its not an Instance map, no need to lookup
            if (map.Type != "Instance")
            {
                return map.Id;
            }

            var mapHash = HashMap(map);

            if (_maps.ContainsKey(mapHash))
            {
                return _maps[mapHash];
            }
            else
            {
                _log.AddError($"Missing lookup. Map: {map.Name}, ID: {map.Id}, Hash: {mapHash}");
                return map.Id; //yolo, it doesnt crash Discord
            }

        }

        private string HashMap(Map map)
        {
            var mapString = $"{map.ContinentId}{map.ContinentRect.TopLeft.X}{map.ContinentRect.TopLeft.Y}{map.ContinentRect.BottomRight.X}{map.ContinentRect.BottomRight.Y}";
            return BitConverter.ToString(new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(mapString))).Replace("-", "").Substring(0, 8).ToLower();
        }
    }
}
