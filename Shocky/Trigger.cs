using System;

namespace Shocky
{
    public class Trigger
    {
        public Guid Id { get; }
        public string TriggerWord { get; set; } = string.Empty;
        public int OperationType { get; set; }
        public int Duration { get; set; }
        public int Intensity { get; set; }

        public Trigger()
        {
            Id = Guid.NewGuid();
        }
    }
}