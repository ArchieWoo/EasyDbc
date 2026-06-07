using System.Globalization;
using System.Text.RegularExpressions;
using EasyDbc.Contracts;
using EasyDbc.Models;

namespace EasyDbc.Parsers.DbcLineParsers
{
    internal class SignalGroupLineParser : ILineParser
    {
        private const string SignalGroupLineStarter = "SIG_GROUP_";
        private const string MessageIdGroup = "MessageId";
        private const string NameGroup = "Name";
        private const string RepetitionsGroup = "Repetitions";
        private const string SignalsGroup = "Signals";

        private readonly string m_signalGroupRegex =
            $@"\s*SIG_GROUP_\s+(?<{MessageIdGroup}>\d+)\s+(?<{NameGroup}>[\w]+)\s+(?<{RepetitionsGroup}>\d+)\s*:\s*(?<{SignalsGroup}>[\w\s]*)\s*;";

        private readonly IParseFailureObserver m_observer;

        public SignalGroupLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.TrimStart().StartsWith(SignalGroupLineStarter) == false)
                return false;

            var match = Regex.Match(line, m_signalGroupRegex);
            if (match.Success)
            {
                var messageId = uint.Parse(match.Groups[MessageIdGroup].Value, CultureInfo.InvariantCulture);
                var signalNames = match.Groups[SignalsGroup].Value
                    .Split(new[] { Helpers.Helper.Space }, StringSplitOptions.RemoveEmptyEntries);

                builder.AddSignalGroup(messageId, new SignalGroup(
                    match.Groups[NameGroup].Value,
                    int.Parse(match.Groups[RepetitionsGroup].Value, CultureInfo.InvariantCulture),
                    signalNames));
            }
            else
                m_observer.UnknownLine();

            return true;
        }
    }
}
