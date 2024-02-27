namespace Shocky.Classes
{
    public class Trigger
    {
        public System.Guid Id { get; }
        public string Phrase { get; set; } = string.Empty;
        public int OperationType { get; set; }
        public int Duration { get; set; }
        public int Intensity { get; set; }

        public Trigger()
        {
            Id = System.Guid.NewGuid();
        }
    }
}
