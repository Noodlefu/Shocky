namespace Shocky.Classes
{
    public class Trigger
    {
        public System.Guid Id { get; }
        public string Phrase { get; set; } = string.Empty;
        public OperationType OperationType { get; set; }
        public int Duration { get; set; } = 1;
        public int Intensity { get; set; } = 1;

        public Trigger()
        {
            Id = System.Guid.NewGuid();
        }
    }
}
