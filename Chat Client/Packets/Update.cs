using Gw2Sharp.Mumble;
using Newtonsoft.Json;
using System;

namespace Chat_Client.Packets
{
    class Update : Packet
    {
#pragma warning disable IDE0051, IDE0052 // Remove unused private members
        [JsonProperty]
        private readonly PacketType type = PacketType.UPDATE;

        [JsonProperty]
        private readonly string name;

        [JsonProperty]
        private readonly bool commander;

        [JsonProperty]
        private readonly string race;

        [JsonProperty]
        private readonly string profession;

        [JsonProperty]
        private readonly string eliteSpec;

        [JsonProperty]
        private readonly int map;

        [JsonProperty]
        private readonly object position;

        [JsonProperty]
        private readonly string serverAddress;
#pragma warning restore IDE0051, IDE0052 // Remove unused private members

        public Update(IGw2MumbleClient data)
        {
            name = data.CharacterName;
            commander = data.IsCommander;
            race = data.Race.ToString();
            profession = data.Profession.ToString();
            eliteSpec = EliteSpec.GetElite(data.Specialization);
            position = new 
            {
                data.PlayerLocationMap.X,
                data.PlayerLocationMap.Y 
            };
            map = data.MapId;
            serverAddress = data.ServerAddress;
        }
    }
}
