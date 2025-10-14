using System.Collections.Generic;
using System.Xml.Linq;

namespace ThwargZone
{
    public class ServerInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Type { get; set; }
        public int PlayerCount { get; set; }
        public string Description { get; set; }
        public string DisplayName => Type?.Contains("PK") == true ? $"{Name} (PK only)" : Name;
    }

    public class PlayerCountData
    {
        public string server { get; set; }
        public int count { get; set; }
        public string date { get; set; }
        public string age { get; set; }
    }
}
