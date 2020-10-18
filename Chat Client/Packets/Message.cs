using Newtonsoft.Json;

namespace Chat_Client.Packets
{
    public class Message : Packet
    {
        [JsonProperty]
        protected private readonly PacketType type = PacketType.MESSAGE;
        [JsonProperty]
        protected private readonly string message;

        public Message(string message)
        {
            this.message = message;
        }
    }
}
