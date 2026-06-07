namespace EasyDbc.Models
{
    public class SignalGroup
    {
        public SignalGroup()
        {
            Name = string.Empty;
            SignalNames = Array.Empty<string>();
        }

        public SignalGroup(string name, int repetitions, IEnumerable<string> signalNames)
        {
            Name = name;
            Repetitions = repetitions;
            SignalNames = signalNames?.ToArray() ?? Array.Empty<string>();
        }

        public string Name { get; set; }
        public int Repetitions { get; set; }
        public string[] SignalNames { get; set; }
    }
}
