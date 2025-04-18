using EasyDbc.Contracts;
using EasyDbc.Generators;
using EasyDbc.Models;
using EasyDbc.Observers;
using EasyDbc.Parsers.DbcLineParsers;
using EasyUDE;

namespace EasyDbc.Parsers
{
    public static class Parser
    {
        private static IParseFailureObserver m_parseObserver = new SilentFailureObserver();
        private static IEnumerable<ILineParser> LineParsers = new List<ILineParser>();

        private static void CreateLineParsers()
        {
            LineParsers = new List<ILineParser>()
            {
                new IgnoreLineParser(m_parseObserver), // Used to skip line we know we want to skip
                new NodeLineParser(m_parseObserver),
                new MessageLineParser(m_parseObserver),
                new CommentLineParser(m_parseObserver),
                new SignalLineParser(m_parseObserver),
                new SignalValueTypeLineParser(m_parseObserver),
                new ValueTableDefinitionLineParser(m_parseObserver),
                new ValueTableLineParser(m_parseObserver),
                new PropertiesDefinitionLineParser(m_parseObserver),
                new PropertiesLineParser(m_parseObserver),
                new EnvironmentVariableLineParser(m_parseObserver),
                new EnvironmentDataVariableLineParser(m_parseObserver),
                new ExtraMessageTransmitterLineParser(m_parseObserver),
                new UnknownLineParser(m_parseObserver) // Used as a catch all 
            };
        }

        public static void SetParsingFailuresObserver(IParseFailureObserver observer)
        {
            m_parseObserver = observer;
        }

        public static Dbc ParseFromPath(string dbcPath)
        {
            using (var fileStream = new FileStream(dbcPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return ParseFromStream(fileStream);
            }
        }
        public static Dbc ParseFromStream(Stream dbcStream)
        {
            ICharsetHandler handler = new CharsetHandler();
            //string encodingName = handler.GetCharset(dbcStream);
            dbcStream = handler.DetectAndConvert(dbcStream, TargetEncoding.UTF_8);

            using (var reader = new StreamReader(dbcStream))
            {
                return ParseFromReader(reader);
            }
        }
        public static void ConvertEncodingFromPath(string dbcPath, string outputFilePath, TargetEncoding targetEncoding)
        {
            ICharsetHandler handler = new CharsetHandler();
            handler.ConvertFileEncoding(dbcPath, outputFilePath, targetEncoding);
        }
        public static Dbc Parse(string dbcText)
        {
            using (var reader = new StringReader(dbcText))
            {
                return ParseFromReader(reader);
            }
        }

        private static Dbc ParseFromReader(TextReader reader)
        {
            CreateLineParsers();
            m_parseObserver.Clear();

            var builder = new DbcBuilder(m_parseObserver);
            var nextLineProvider = new NextLineProvider(reader);

            while (reader.Peek() >= 0)
                ParseLine(reader.ReadLine(), builder, nextLineProvider);

            return builder.Build();
        }

        private static void ParseLine(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            m_parseObserver.CurrentLine++;
            if (string.IsNullOrWhiteSpace(line))
                return;

            foreach (var parser in LineParsers)
            {
                if (parser.TryParse(line, builder, nextLineProvider))
                    break;
            }
        }
    }
}