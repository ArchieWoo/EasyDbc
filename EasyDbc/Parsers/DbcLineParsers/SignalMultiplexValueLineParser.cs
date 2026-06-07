using System.Globalization;
using System.Text.RegularExpressions;
using EasyDbc.Contracts;
using EasyDbc.Models;

namespace EasyDbc.Parsers.DbcLineParsers
{
    internal class SignalMultiplexValueLineParser : ILineParser
    {
        private const string SignalMultiplexValueLineStarter = "SG_MUL_VAL_";
        private const string MessageIdGroup = "MessageId";
        private const string SignalNameGroup = "SignalName";
        private const string MultiplexorGroup = "Multiplexor";
        private const string RangesGroup = "Ranges";

        private readonly string m_signalMultiplexValueRegex =
            $@"\s*SG_MUL_VAL_\s+(?<{MessageIdGroup}>\d+)\s+(?<{SignalNameGroup}>[\w]+)\s+(?<{MultiplexorGroup}>[\w]+)\s+(?<{RangesGroup}>[\d\-,\s]+)\s*;";

        private readonly IParseFailureObserver m_observer;

        public SignalMultiplexValueLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.TrimStart().StartsWith(SignalMultiplexValueLineStarter) == false)
                return false;

            var match = Regex.Match(line, m_signalMultiplexValueRegex);
            if (match.Success && TryParseRanges(match.Groups[RangesGroup].Value, out var ranges))
            {
                var messageId = uint.Parse(match.Groups[MessageIdGroup].Value, CultureInfo.InvariantCulture);
                builder.AddSignalMultiplexRange(
                    messageId,
                    match.Groups[SignalNameGroup].Value,
                    new SignalMultiplexRange(match.Groups[MultiplexorGroup].Value, ranges));
            }
            else
                m_observer.UnknownLine();

            return true;
        }

        private static bool TryParseRanges(string text, out List<MultiplexRange> ranges)
        {
            ranges = new List<MultiplexRange>();
            var parts = text.Split(new[] { Helpers.Helper.Comma }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var bounds = part.Trim().Split(new[] { "-" }, StringSplitOptions.None);
                if (bounds.Length != 2 ||
                    int.TryParse(bounds[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var from) == false ||
                    int.TryParse(bounds[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var to) == false)
                {
                    ranges.Clear();
                    return false;
                }

                ranges.Add(new MultiplexRange(from, to));
            }

            return ranges.Count > 0;
        }
    }
}
