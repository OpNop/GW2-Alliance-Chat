using Gw2Sharp.Mumble;
using Newtonsoft.Json;

namespace Chat_Client.Packets
{
    class Update : Packet
    {
        [JsonProperty]
        private readonly PacketType type = PacketType.UPDATE;

        public Update(IGw2MumbleClient data)
        {

        }
    }
}
