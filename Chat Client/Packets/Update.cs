using Gw2Sharp.Mumble;
using Newtonsoft.Json;
using System;

namespace Chat_Client.Packets
{
    class Update : Packet
    {
        [JsonProperty]
        protected private readonly PacketType type = PacketType.UPDATE;

        [JsonProperty]
        protected private readonly string name;

        [JsonProperty]
        protected private readonly bool commander;

        [JsonProperty]
        protected private readonly string race;

        [JsonProperty]
        protected private readonly string profession;

        [JsonProperty]
        protected private readonly string eliteSpec;

        [JsonProperty]
        protected private readonly int map;

        [JsonProperty]
        protected private readonly object position;

        [JsonProperty]
        protected private readonly string serverAddress;

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
