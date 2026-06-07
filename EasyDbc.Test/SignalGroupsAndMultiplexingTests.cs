using EasyDbc.Generators;
using EasyDbc.Models;
using EasyDbc.Parsers;

namespace EasyDbc.Test
{
    public class SignalGroupsAndMultiplexingTests
    {
        [Test]
        public void ExtendedMultiplexingLinesAreParsed()
        {
            var dbc = Parser.ParseFromPath(@"..\..\..\..\DbcFiles\ext_multiplexed.dbc");

            var message = dbc.Messages.Single(message => message.Name == "ExtMX_Message");
            var signal = message.Signals.Single(signal => signal.Name == "S1_m");

            Assert.Multiple(() =>
            {
                Assert.That(signal.Multiplexing, Is.EqualTo("m0M"));
                Assert.That(signal.MultiplexRanges, Has.Count.EqualTo(1));
                Assert.That(signal.MultiplexRanges[0].MultiplexorSignalName, Is.EqualTo("S0_m"));
                Assert.That(signal.MultiplexRanges[0].Ranges, Has.Count.EqualTo(2));
                Assert.That(signal.MultiplexRanges[0].Ranges[0].From, Is.EqualTo(0));
                Assert.That(signal.MultiplexRanges[0].Ranges[0].To, Is.EqualTo(0));
                Assert.That(signal.MultiplexRanges[0].Ranges[1].From, Is.EqualTo(2));
                Assert.That(signal.MultiplexRanges[0].Ranges[1].To, Is.EqualTo(2));
            });
        }

        [Test]
        public void SignalGroupsAreParsed()
        {
            var dbc = Parser.Parse(@"
BO_ 100 Example: 8 ECU
 SG_ Mode M : 0|2@1+ (1,0) [0|3] """" ECU
 SG_ Speed m1 : 8|8@1+ (1,0) [0|255] ""km/h"" ECU
 SG_ Torque m1 : 16|8@1- (1,0) [-128|127] ""Nm"" ECU
SIG_GROUP_ 100 Powertrain 2 : Speed Torque;
");

            var message = dbc.Messages.Single();

            Assert.Multiple(() =>
            {
                Assert.That(message.SignalGroups, Has.Count.EqualTo(1));
                Assert.That(message.SignalGroups[0].Name, Is.EqualTo("Powertrain"));
                Assert.That(message.SignalGroups[0].Repetitions, Is.EqualTo(2));
                Assert.That(message.SignalGroups[0].SignalNames, Is.EqualTo(new[] { "Speed", "Torque" }));
            });
        }

        [Test]
        public void SignalGroupsAndExtendedMultiplexingAreGenerated()
        {
            var message = new Message
            {
                ID = 100,
                Name = "Example",
                DLC = 8,
                Transmitter = "ECU",
                Signals = new List<Signal>
                {
                    new Signal { Name = "Mode", Multiplexing = "M", StartBit = 0, Length = 2, ByteOrder = 1, ValueType = DbcValueType.Unsigned, Factor = 1, Unit = string.Empty, Receiver = new[] { "ECU" } },
                    new Signal
                    {
                        Name = "Speed",
                        Multiplexing = "m1",
                        StartBit = 8,
                        Length = 8,
                        ByteOrder = 1,
                        ValueType = DbcValueType.Unsigned,
                        Factor = 1,
                        Unit = "km/h",
                        Receiver = new[] { "ECU" },
                        MultiplexRanges = new List<SignalMultiplexRange>
                        {
                            new SignalMultiplexRange("Mode", new[] { new MultiplexRange(1, 1), new MultiplexRange(3, 5) })
                        }
                    }
                },
                SignalGroups = new List<SignalGroup>
                {
                    new SignalGroup("Powertrain", 1, new[] { "Mode", "Speed" })
                }
            };
            foreach (var signal in message.Signals)
            {
                signal.Parent = message;
            }

            var dbc = new Dbc(new[] { new Node { Name = "ECU" } }, new[] { message }, Array.Empty<EnvironmentVariable>(), Array.Empty<CustomProperty>());
            using var writer = new StringWriter();

            DbcGenerator.WriteToWriter(dbc, writer);

            var text = writer.ToString();
            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("SIG_GROUP_ 100 Powertrain 1 : Mode Speed;"));
                Assert.That(text, Does.Contain("SG_MUL_VAL_ 100 Speed Mode 1-1, 3-5;"));
            });
        }

        [Test]
        public void ExcelRoundTripKeepsOptionalSignalGroupAndMultiplexingColumns()
        {
            var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "signal_groups_roundtrip.xlsx");
            var message = new Message
            {
                ID = 100,
                Name = "Example",
                DLC = 8,
                Transmitter = "ECU",
                Signals = new List<Signal>
                {
                    new Signal { Name = "Mode", Multiplexing = "M", StartBit = 0, Length = 2, ByteOrder = 1, ValueType = DbcValueType.Unsigned, Factor = 1, Unit = string.Empty, Receiver = new[] { "ECU" } },
                    new Signal
                    {
                        Name = "Speed",
                        Multiplexing = "m1",
                        StartBit = 8,
                        Length = 8,
                        ByteOrder = 1,
                        ValueType = DbcValueType.Unsigned,
                        Factor = 1,
                        Unit = "km/h",
                        Receiver = new[] { "ECU" },
                        MultiplexRanges = new List<SignalMultiplexRange>
                        {
                            new SignalMultiplexRange("Mode", new[] { new MultiplexRange(1, 1) })
                        }
                    }
                },
                SignalGroups = new List<SignalGroup>
                {
                    new SignalGroup("Powertrain", 1, new[] { "Mode", "Speed" })
                }
            };
            foreach (var signal in message.Signals)
            {
                signal.Parent = message;
            }

            var sourceDbc = new Dbc(new[] { new Node { Name = "ECU" } }, new[] { message }, Array.Empty<EnvironmentVariable>(), Array.Empty<CustomProperty>());
            var generator = new ExcelGenerator();
            generator.WriteToFile(sourceDbc, outputPath);

            var parser = new ExcelParser();
            parser.ParseFirstSheetFromPath(outputPath, out var parsedDbc);

            var parsedMessage = parsedDbc.Messages.Single();
            var speed = parsedMessage.Signals.Single(signal => signal.Name == "Speed");
            Assert.Multiple(() =>
            {
                Assert.That(parsedMessage.SignalGroups, Has.Count.EqualTo(1));
                Assert.That(parsedMessage.SignalGroups[0].SignalNames, Is.EqualTo(new[] { "Mode", "Speed" }));
                Assert.That(speed.Multiplexing, Is.EqualTo("m1"));
                Assert.That(speed.MultiplexRanges[0].MultiplexorSignalName, Is.EqualTo("Mode"));
                Assert.That(speed.MultiplexRanges[0].Ranges[0].From, Is.EqualTo(1));
            });
        }

        [Test]
        public void ExcelGeneratorCanSkipOptionalSignalGroupAndMultiplexingColumns()
        {
            var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "signal_groups_disabled.xlsx");
            var sourceDbc = CreateSignalGroupSampleDbc();

            var generator = new ExcelGenerator { IncludeSignalGroupColumns = false };
            generator.WriteToFile(sourceDbc, outputPath);

            var parser = new ExcelParser();
            parser.ParseFirstSheetFromPath(outputPath, out var parsedDbc);

            var parsedMessage = parsedDbc.Messages.Single();
            var speed = parsedMessage.Signals.Single(signal => signal.Name == "Speed");
            Assert.Multiple(() =>
            {
                Assert.That(parsedMessage.SignalGroups, Is.Empty);
                Assert.That(speed.Multiplexing, Is.Null.Or.Empty);
                Assert.That(speed.MultiplexRanges, Is.Empty);
            });
        }

        [Test]
        public void ExcelParserCanIgnoreOptionalSignalGroupAndMultiplexingColumns()
        {
            var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "signal_groups_parse_disabled.xlsx");
            var sourceDbc = CreateSignalGroupSampleDbc();
            var generator = new ExcelGenerator();
            generator.WriteToFile(sourceDbc, outputPath);

            var parser = new ExcelParser { ParseSignalGroupColumns = false };
            parser.ParseFirstSheetFromPath(outputPath, out var parsedDbc);

            var parsedMessage = parsedDbc.Messages.Single();
            var speed = parsedMessage.Signals.Single(signal => signal.Name == "Speed");
            Assert.Multiple(() =>
            {
                Assert.That(parsedMessage.SignalGroups, Is.Empty);
                Assert.That(speed.Multiplexing, Is.Null.Or.Empty);
                Assert.That(speed.MultiplexRanges, Is.Empty);
            });
        }

        private static Dbc CreateSignalGroupSampleDbc()
        {
            var message = new Message
            {
                ID = 100,
                Name = "Example",
                DLC = 8,
                Transmitter = "ECU",
                Signals = new List<Signal>
                {
                    new Signal { Name = "Mode", Multiplexing = "M", StartBit = 0, Length = 2, ByteOrder = 1, ValueType = DbcValueType.Unsigned, Factor = 1, Unit = string.Empty, Receiver = new[] { "ECU" } },
                    new Signal
                    {
                        Name = "Speed",
                        Multiplexing = "m1",
                        StartBit = 8,
                        Length = 8,
                        ByteOrder = 1,
                        ValueType = DbcValueType.Unsigned,
                        Factor = 1,
                        Unit = "km/h",
                        Receiver = new[] { "ECU" },
                        MultiplexRanges = new List<SignalMultiplexRange>
                        {
                            new SignalMultiplexRange("Mode", new[] { new MultiplexRange(1, 1) })
                        }
                    }
                },
                SignalGroups = new List<SignalGroup>
                {
                    new SignalGroup("Powertrain", 1, new[] { "Mode", "Speed" })
                }
            };
            foreach (var signal in message.Signals)
            {
                signal.Parent = message;
            }

            return new Dbc(new[] { new Node { Name = "ECU" } }, new[] { message }, Array.Empty<EnvironmentVariable>(), Array.Empty<CustomProperty>());
        }
    }
}
