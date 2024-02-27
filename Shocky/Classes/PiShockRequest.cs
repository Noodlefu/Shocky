namespace Shocky.Classes
{
    internal class PiShockRequest
    {
        public required string Username { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public int Intensity { get; set; }
        public int Duration { get; set; }
        public required string ApiKey { get; set; }
        public int Op { get; set; }
    }
}