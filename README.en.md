# EasyDbc

<p align="right">
  <a >English</a> |
  <a href="./README.zh.md">中文</a> |
  <a href="./README.de.md">Deutsch</a> |
  <a href="./README.kr.md">한국어</a> |
  <a href="./README.jp.md">日本語</a> |
  <a href="./README.fr.md">Français</a>
</p>

[![](https://img.shields.io/nuget/dt/EasyDbc?color=004880&label=downloads&logo=NuGet)](https://www.nuget.org/packages/EasyDbc/)
[![](https://img.shields.io/nuget/vpre/EasyDbc?color=%23004880&label=NuGet&logo=NuGet)](https://www.nuget.org/packages/EasyDbc/)
[![GitHub](https://img.shields.io/github/license/Vico-wu/EasyDbc?color=%231281c0)](LICENSE)

## Introduction  
Brief introduction to the project's features and goals.  
This project is developed based on [`DbcParserLib`](https://github.com/EFeru/DbcParser) and extends the following functionalities:

- **DBC File Merging**: Supports merging multiple DBC files.
- **DBC File Generation**: Can generate new DBC files based on requirements.
- **Excel File Parsing to DBC**: Provides functionality to parse Excel files into DBC files with flexible mapping and conversion.
- **Excel File Generation**: Supports generating Excel files from DBC data.
- **Custom Excel Parsing and Generation Logic**: Offers flexible custom logic for parsing and generating Excel files to meet different needs.

These extended functionalities make the project more flexible and efficient in handling DBC and Excel files, enhancing vehicle network management and signal processing capabilities.

---

## Features  
- **Parse DBC Files**: Supports extracting signals, messages, and node information.  
- **Data Display**: Visually displays parsing results through an interactive interface.  
- **Format Validation**: Flexible parsing of both old and new Excel formats.  
- **Extensibility**: Provides flexible custom logic for Excel file parsing and generation.  

---

## Plugin References

This project refers to the [`NPOI`](https://github.com/dotnetcore/NPOI) plugin when implementing Excel parsing and generation. `NPOI` is an open-source .NET library for handling Microsoft Office file formats, including Excel, Word, and PowerPoint. The plugin is powerful and easy to use, particularly suited for various Excel file processing needs.

In this project, I primarily used the following features of the `NPOI` plugin:

- **Grouping**: Supports grouping data based on specified rules for better display and management.
- **Drop-down Menus**: Implements drop-down menu functionality in Excel cells, simplifying user input and ensuring data consistency.
- **Validation**: Uses the data validation feature provided by `NPOI` to ensure that the data in Excel files complies with specified rules.
- **Background Color**: Customizes the background color of Excel cells for better readability and visual effect.
- **Cell Formatting**: Supports setting cell formats, such as number formats, date formats, etc., to ensure correct data display.

By using `NPOI`, this project can handle Excel files more flexibly, implement complex functionality requirements, and enhance data processing capabilities.

---

## Project Structure  
```plaintext
Project Root Directory/
├── EasyDbc/                # Source code folder
│   ├── Assets/             # Images and other auxiliary resources
│   ├── Contracts/          # Interface files
│   ├── Helpers/            # Extensions
│   ├── Models/             # Data models
│   ├── Observers/          # Error classes
│   ├── Parsers/            # Parser classes
│   ├── Generators/         # File generation classes
│   └── EasyDbc.csproj      # Project file
├── DbcFiles/               # Resource files
├── EasyDbc.Benchmark/      # Performance test folder
├── EasyDbc.Test/           # Unit test folder
├── EasyDbc.Demo/           # Demo project folder
├── README.zh_CN.md         # Chinese project documentation file
├── README.md               # Project documentation file
└── LICENSE                 # License file
```
---

# Quick Start

## Program Demo

Here are some screenshots demonstrating the features and results of the project.

**Example Description Screenshot:**

![SoftwareDescription](https://raw.githubusercontent.com/Vico-wu/EasyDbc/master/EasyDbc/Assets/DemoDescription.jpg)


---

## Feature 1: **DBC File Parsing**

Parse the DBC file by specifying the path and successfully load its contents.

**Run Screenshot:**

![DBC Parsing](https://raw.githubusercontent.com/Vico-wu/EasyDbc/master/EasyDbc/Assets/Overview.jpg)

**Example Explanation:**
- Input: Select a well-formed DBC file.
- Output: Successfully parses the file and displays signal list and message information.

---

## Feature 2: **Excel File Parsing and Generation**

Supports parsing Excel files to DBC data models, and also generating Excel files based on user needs.

**Run Screenshot:**

![Excel Parsing](https://raw.githubusercontent.com/Vico-wu/EasyDbc/master/EasyDbc/Assets/ExcelView.jpg)

**Example Explanation:**
- Feature Highlights:
  - Custom logic parsing support.
  - Flexibly generate Excel files with drop-down menus, data validation, and styling.

---

## Feature 3: **DBC File Merging**

Merges multiple DBC files into one, resolving duplicate signals and conflicts.。

**Run Screenshot:**

![DBC Merge](https://raw.githubusercontent.com/Vico-wu/EasyDbc/master/EasyDbc/Assets/DbcMergeView.jpg)

**Example Explanation:**
- Input: Multiple DBC files to merge.
- Output: Generates a merged DBC file with customizable merging rules.

---


## Summary

The above showcases some core features of the project. If you're interested in any specific features, please refer to the following sections for detailed usage instructions.

### Add Namespace

```cs
// For DBC file and Excel file generation
using EasyDbc.Generators;
// For special signal parsing (e.g., signal.Parent.CycleTime(out var cycleTime))
using EasyDbc.Helpers;
// For associated models
using EasyDbc.Models;
// For DBC and Excel file parsing
using EasyDbc.Parsers;

```
---

### Parsing DBC and Excel Files into `Dbc` Classes

```cs
private bool TryParsingToFile(string path, out Dbc dbc)
{
    string extension = Path.GetExtension(path)?.ToLower();
    // Parse DBC file based on file path
    if (extension == ".dbc")
    {
        dbc = Parser.ParseFromPath(path);
        if (dbc != null)
        {
            return true;
        }
        return true;
    }
    else if (extension == ".xls" || extension == ".xlsx")
    {
    // Parse Excel file based on file path
        ExcelParser excelParser = new ExcelParser();
        ExcelParserState result = excelParser.ParseFirstSheetFromPath(path, out Dbc dbcOutput);
        if (result == ExcelParserState.Success)
        {
            dbc = dbcOutput;
            return true;
        }
    }
    dbc = null;
    return false;
}

```
---

### Merging DBC Functionality Display: Can Be Customized Based on Actual Needs
**⚠️ Important Note** 

Currently, the DBC merging feature will prioritize the first added message, invalidating any subsequent duplicate messages.

```cs
private bool ParsingAndMergeDbc()
{
    Nodes = string.Empty;
    Messages.Clear();
    _mergedDbc = null;
    List<Dbc> parsingResult = new List<Dbc>();
    if (!string.IsNullOrEmpty(FilePath1))
    {
        if (TryParsingToFile(FilePath1, out Dbc dbc))
        {
            parsingResult.Add(dbc);
        }
    }
    if (!string.IsNullOrEmpty(FilePath2))
    {
        if (TryParsingToFile(FilePath2, out Dbc dbc))
        {
            parsingResult.Add(dbc);
        }
    }
    if (!string.IsNullOrEmpty(FilePath3))
    {
        if (TryParsingToFile(FilePath3, out Dbc dbc))
        {
            parsingResult.Add(dbc);
        }
    }
    bool result = DbcGenerator.MergeDbc(parsingResult, out _mergedDbc);
    if (result)
    {
        foreach (Node node in _mergedDbc.Nodes)
        {
            Nodes = string.Join("; ", _mergedDbc.Nodes.Select(node => node.Name));
        }
    }
    return result;
}

```
---
### **DBC File Parsing**
Use the static class `Parser` to parse DBC files. Choose one parsing method:
```cs
// Parse by file path
Dbc dbc = Parser.ParseFromPath("C:\\your_dbc_file.dbc");
// Parse by stream
Dbc dbc = Parser.ParseFromStream(File.OpenRead("C:\\your_dbc_file.dbc")); 
// Parse from a string
Dbc dbc = Parser.Parse("a dbc as string");

```
---

### **Handling DBC File Objects**

The `DBC` object contains two collections, `Messages` and `Nodes`, both of which are of type `IEnumerable<T>`. You can use standard LINQ to access, iterate, and query them.

For example, get all messages with IDs greater than 100 and more than 2 signals:
```cs
var filteredSelection = dbc
			.Messages
			.Where(m => m.ID > 100 && m.Signals.Count > 2)
			.ToArray();
```
---

### **Parsing Error Management**

Use the `IParseFailureObserver` interface to handle syntax errors during parsing. It provides methods for:
- General syntax errors (e.g., missing ;, ', ,)
- Duplicate object definitions (e.g., messages with the same ID, nodes, signals, custom properties)
- Missing object definitions (e.g., custom properties assigned before declaration)
- Value consistency (e.g., custom property values exceeding min/max range)

The library offers two implementations:

1. `SilentFailureObserver`: Default implementation that suppresses errors during parsing.
2. `SimpleFailureObserver`: A simple observer that logs any errors:
    - Unknown syntax: No corresponding TAG syntax
    - [TAG] Syntax error: Syntax error in a specific TAG
    - Duplicated object: The parser found (and ignored) a duplicated object
    - Object Not found: An object is declared or referenced before it is defined
    - Property value out of bound: The value assigned to a property is below/above the minimum/maximum value defined for that property
    - Property value out of index: The declared index value is not acceptable (for properties that support accessing values by index, like enum values)

The list of errors can be retrieved via the `GetErrorList()` method:
```cs
    // Comment out these two lines to remove error parsing management (errors will be silently handled)
    // You can provide your own IParseFailureObserver implementation to customize error parsing management
    var failureObserver = new SimpleFailureObserver();
    Parser.SetParsingFailuresObserver(failureObserver);

    var dbc = Parser.ParseFromPath(filePath);
    var errors = failureObserver.GetErrorList();
```
---

### Signal Packing and Unpacking

### **Simple Application Example**

To pack and unpack signals, you can use the static Packer class. 

Example of packing/unpacking a 14-bit signal with a minimum value of `-61.92` and a maximum value of `101.91`:

```cs
Signal sig = new Signal
{
  sig.Length = 14,
  sig.StartBit = 2,
  sig.IsSigned = 1,
  sig.ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
  sig.Factor = 0.01,
  sig.Offset = 20
};

// Packing a signal to send
ulong TxMsg = Packer.TxSignalPack(-34.3, sig);

// This unpacks the received signal and calculates the corresponding physical value
double val = Packer.RxSignalUnpack(TxMsg, sig);

```

You can also pack multiple signals before sending them over CAN:
```cs
ulong TxMsg = 0;
TxMsg |= Packer.TxSignalPack(value1, sig1);
TxMsg |= Packer.TxSignalPack(value2, sig2);
TxMsg |= Packer.TxSignalPack(value3, sig3);
// ...
// Send TxMsg to the CAN bus.
```

Ensure that the signals do not overlap by correctly specifying `Length` and `StartBit`.

### **Multiplexed Signal Application Example**
A message may contain multiplexed data, meaning its layout can change depending on the value of a multiplexer. 
The `Packer` class does not handle multiplexing, so users need to check if a given message indeed contains the signal.

For example, consider the following dbc file content:
```
BO_ 568 UI_driverAssistRoadSign: 8 GTW
 SG_ UI_roadSign M : 0|8@1+ (1,0) [0|0] ""  DAS
 SG_ UI_dummyData m0 : 8|1@1+ (1,0) [0|0] "" Vector__XXX
 SG_ UI_stopSignStopLineDist m1 : 8|10@1+ (0.25,-8) [-8|247.5] "m" Vector__XXX
```

The signal `UI_dummyData` is only available when `UI_roadSign` is 0, and `UI_stopSignStopLineDist` is only available when `UI_roadSign` is 1. 
You can access multiplexing information by calling the following method:
```cs
var multiplexingInfo = signal.MultiplexingInfo();
if(multiplexingInfo.Role == MultiplexingRole.Multiplexor)
{
	// This is a multiplexer!
}
else if(multiplexingInfo.Role == MultiplexingRole.Multiplexed)
{
	Console.WriteLine($"This signal is multiplexed and will be available when multiplexor value is {multiplexingInfo.Group}");
}
```
You can also use the extension method to check if a message contains multiplexed signals:
```cs
if(message.IsMultiplexed())
{
	// ...
}
```

### DBC File Encoding Conversion
For GB2312 conversion, the latest standard GB18030 is recommended, which Notepad++ can accurately recognize. This standard is backward compatible with GB2312.
Currently supported transcoding formats are as follows:


    ASCII,
    UTF_8,          // Unicode Transformation Format (8-bit)
    UTF_16LE,       // Unicode Transformation Format (16-bit, Little Endian)
    UTF_16BE,       // Unicode Transformation Format (16-bit, Big Endian)
    UTF_32BE,       // Unicode Transformation Format (32-bit, Big Endian)
    UTF_32LE,       // Unicode Transformation Format (32-bit, Little Endian)
    windows_1251,   // Cyrillic (Windows)
    windows_1252,   // Western European (Windows)
    windows_1253,   // Greek (Windows)
    windows_1255,   // Hebrew (Windows)
    Big5,          // Traditional Chinese
    EUC_KR,        // Korean (Extended Unix Code)
    EUC_JP,        // Japanese (Extended Unix Code)
    ISO_2022_JP,   // Japanese (ISO Standard)
    ISO_2022_CN,   // Chinese (ISO Standard)
    ISO_2022_KR,   // Korean (ISO Standard)
    HZ_GB_2312,    // Chinese (GB2312 in HZ encoding)
    Shift_JIS,     // Japanese (Shift JIS)
    x_mac_cyrillic,// Cyrillic (Mac OS)
    KOI8_R,        // Cyrillic (KOI8-R)
    IBM855,        // Cyrillic (IBM)
    IBM866,        // Cyrillic (alternative IBM encoding)
    ISO_8859_2,    // Central European
    ISO_8859_5,    // Cyrillic
    ISO_8859_7,    // Greek
    ISO_8859_8,    // Hebrew
    GBK,           // Chinese (extended GB2312)
    GB2312,        // Simplified Chinese
    GB18030        // Current Chinese national standard


```cs
string sourceFilePath = @"..\..\..\..\DbcFiles\SampleDbc_UTF8.dbc";
string outputFilePath = @"..\..\..\..\DbcFiles\OutputDbc_GB2312.dbc";
Parser.ConvertEncodingFromPath(sourceFilePath, outputFilePath, TargetEncoding.GB18030);

---
# Contributing

Welcome contributions! Feel free to submit pull requests to improve this library.

---