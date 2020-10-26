using Chat_Client.utils.Log;
using Newtonsoft.Json;

namespace Chat_Client.Packets
{
    public class Packet
    {
        private static readonly Logger _log = Logger.getInstance();

        public string Send()
        {
            var packet = JsonConvert.SerializeObject(this);
            _log.AddDebug(packet);
            return packet;
        }
    }
}
