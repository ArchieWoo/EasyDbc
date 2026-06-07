namespace EasyDbc.Models
{
    public readonly struct MultiplexRange
    {
        public MultiplexRange(int from, int to)
        {
            From = from;
            To = to;
        }

        public int From { get; }
        public int To { get; }
    }

    public class SignalMultiplexRange
    {
        public SignalMultiplexRange()
        {
            MultiplexorSignalName = string.Empty;
            Ranges = new List<MultiplexRange>();
        }

        public SignalMultiplexRange(string multiplexorSignalName, IEnumerable<MultiplexRange> ranges)
        {
            MultiplexorSignalName = multiplexorSignalName;
            Ranges = ranges?.ToList() ?? new List<MultiplexRange>();
        }

        public string MultiplexorSignalName { get; set; }
        public List<MultiplexRange> Ranges { get; set; }
    }
}
