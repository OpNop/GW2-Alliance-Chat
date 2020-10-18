using Newtonsoft.Json;

namespace Chat_Client.Packets
{
    public class Connect : Packet
    {
        [JsonProperty]
        protected private readonly PacketType type = PacketType.CONNECT;
        [JsonProperty]
        protected private readonly string version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        [JsonProperty]
        protected private readonly string key;

        public Connect(string key)
        {
            this.key = key;
        }
    }
}
