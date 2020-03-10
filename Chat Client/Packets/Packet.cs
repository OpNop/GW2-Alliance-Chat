using Newtonsoft.Json;

namespace Chat_Client.Packets
{
    public class Packet
    {
        public string Send()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
