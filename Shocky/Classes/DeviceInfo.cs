namespace Shocky.Classes
{    
    public class DeviceInfo
    {
        public int clientId { get; set; }
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public bool paused { get; set; }
        public int maxIntensity { get; set; }
        public int maxDuration { get; set; }
        public bool online { get; set; }
        public string error { get; set;} = string.Empty;
    }
}