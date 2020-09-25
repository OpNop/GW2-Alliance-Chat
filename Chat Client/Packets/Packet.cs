using Newtonsoft.Json;
using System;

namespace Chat_Client.Packets
{
    public class Packet
    {
        public string Send()
        {
            var packet = JsonConvert.SerializeObject(this);
            Console.WriteLine($"SEND> {packet}");
            return packet;
        }
    }
}
