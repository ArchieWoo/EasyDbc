using EasyDbc.Generators;
using EasyDbc.Models;
using EasyDbc.Parsers;

namespace EasyDbc.Test
{
    public class ExcelGeneratorTests
    {
        [Test]
        public void SimpleWriteToExcelXlsTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";        
            string outputPath = @"..\..\..\..\DbcFiles\tesla_can.xls";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath);
            Assert.That(File.Exists(outputPath), Is.True);
        }
        [Test]
        public void WriteToExcelByIdSordTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";
            string outputPath = @"..\..\..\..\DbcFiles\tesla_IdSorted_can.xls";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath,"Matrix",DbcOrderBy.Id);
            Assert.That(File.Exists(outputPath), Is.True);
        }
        [Test]
        public void WriteToExcelByTransmitterSordTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";
            string outputPath = @"..\..\..\..\DbcFiles\tesla_TransmitterSorted_can.xls";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath, "Matrix", DbcOrderBy.Transmitter);
            Assert.That(File.Exists(outputPath), Is.True);
        }
        [Test]
        public void WriteToExcelByNameSordTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";
            string outputPath = @"..\..\..\..\DbcFiles\tesla_NameSorted_can.xls";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath, "Matrix", DbcOrderBy.Name);
            Assert.That(File.Exists(outputPath), Is.True);
        }
        [Test]
        public void SimpleWriteToExcelXlsxTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";
            string outputPath = @"..\..\..\..\DbcFiles\tesla_can.xlsx";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath);
            Assert.That(File.Exists(outputPath), Is.True);
        }
        [Test]
        public void WriteToExcelXlsPathTest()
        {
            string path = @"..\..\..\..\DbcFiles\tesla_can.dbc";
            string outputPath = @"..\..\..\..\DbcFile\tesla_can.xls";//ErrorPath
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath);
            Assert.That(File.Exists(outputPath),Is.True );
        }
        [Test]
        public void ColumnIndexConfictionCheckTest()
        {
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.UpdateColumnConfig(DictionaryColumnKey.MessageName, 1);
            Assert.That(excelGenerator.CheckColumnIndexConfiction(out List<int> confictionIndexList), Is.True);
        }
        [Test]
        public void ExcelParsingWithEncodingTest()
        {
            string path = @"..\..\..\..\DbcFiles\SampleDbc_GB2312.dbc";
            string outputPath = @"..\..\..\..\DbcFiles\Generated_SampleDbc_GB2312.xlsx";
            var dbc = Parser.ParseFromPath(path);
            ExcelGenerator excelGenerator = new ExcelGenerator();
            excelGenerator.WriteToFile(dbc, outputPath);
            Assert.That(File.Exists(outputPath), Is.True);
        }

    }
}
