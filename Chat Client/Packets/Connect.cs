using Newtonsoft.Json;

namespace Chat_Client.Packets
{
    public class Connect : Packet
    {
#pragma warning disable IDE0051, IDE0052 // Remove unread private members
        [JsonProperty]
        private readonly PacketType type = PacketType.CONNECT;
        [JsonProperty]
        private readonly string version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        [JsonProperty]
        private readonly string key;
#pragma warning restore IDE0051, IDE0052 // Remove unread private members

        public Connect(string key)
        {
            this.key = key;
        }
    }
}
