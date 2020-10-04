using Newtonsoft.Json;

namespace Chat_Client.Packets
{
    public class Message : Packet
    {
#pragma warning disable IDE0051, IDE0052 // Remove unread private members
        [JsonProperty]
        private readonly PacketType type = PacketType.MESSAGE;
        [JsonProperty]
        private readonly string message;
#pragma warning restore IDE0051, IDE0052 // Remove unread private members

        public Message(string message)
        {
            this.message = message;
        }
    }
}
