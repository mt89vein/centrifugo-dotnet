using System;

namespace Centrifugo.Client.Events
{
    public class ConnectedEvent
    {
        public Guid? Client { get; set; }

        public string? Version { get; set; }

        public bool? Expires { get; set; }

        public uint? Ttl { get; set; }
    }
}