# EasyDbc

<p align="right">
  <a href="./README.md">English</a> |
  <a >中文</a> |
  <a href="./docs/README_de.md">Deutsch</a> |
  <a href="./docs/README_kr.md">한국어</a> |
  <a href="./docs/README_jp.md">日本語</a> |
  <a href="./docs/README_fr.md">Français</a>
</p>


## 简介  
简要介绍项目的功能和目标。  
本项目基于 [`DbcParserLib`](https://github.com/EFeru/DbcParser) 开发，在此基础上扩展了以下功能：

- **DBC 文件合并**：支持多个 DBC 文件的合并操作。
- **DBC 文件生成功能**：可以根据需求生成新的 DBC 文件。
- **Excel 文件解析为 DBC**：提供将 Excel 文件解析为 DBC 文件的功能，支持灵活的映射和转换。
- **Excel 文件生成**：支持从 DBC 数据生成 Excel 文件。
- **自定义 Excel 解析和生成逻辑**：针对 Excel 文件解析和生成，提供灵活的自定义逻辑，满足不同需求。

这些扩展功能使得项目能够更加灵活和高效地处理 DBC 文件和 Excel 文件，提升了车辆网络管理和信号处理的能力。


---

## 功能特性  
- **解析 DBC 文件**：支持提取信号、消息和节点信息。  
- **数据展示**：通过交互界面直观展示解析结果。  
- **格式校验**：可以灵活的解析Excel的新旧格式。  
- **扩展性**：Excel 文件解析和生成，提供灵活的自定义逻辑。  

---
## 插件参考

本项目在实现 Excel 解析和生成功能时，参照了 [`NPOI`](https://github.com/dotnetcore/NPOI) 插件。`NPOI` 是一个开源的 .NET 库，用于处理 Microsoft Office 格式的文件（包括 Excel、Word 和 PowerPoint）。该插件功能强大且易于使用，特别适用于处理 Excel 文件的各种需求。

在本项目中，我主要使用了 `NPOI` 插件的以下功能：

- **分组**：支持将数据按指定规则分组，以便于更好地展示和管理。
- **下拉菜单**：实现了 Excel 单元格中的下拉菜单功能，简化用户输入，确保数据一致性。
- **验证**：通过 `NPOI` 提供的数据验证功能，确保 Excel 文件中的数据符合指定规则。
- **设置背景色**：能够自定义 Excel 单元格的背景色，提升可读性和视觉效果。
- **设置单元格格式**：支持设置单元格的格式，如数字格式、日期格式等，确保数据的正确显示。

通过使用 `NPOI`，本项目能够更加灵活地处理 Excel 文件，实现复杂的功能需求，提升了数据的处理能力。

---

## 项目结构  
```plaintext
项目根目录/
├── EasyDbc/                # 源代码文件夹
│   ├── Assets/             # 图片等附加资源
│   ├── Contracts/          # 接口文件
│   ├── Helpers/            # 扩展
│   ├── Models/             # 数据模型
│   ├── Observers/          # 错误类
│   ├── Parsers/            # 解析类
│   ├── Generators/         # 文件生成类
│   └── EasyDbc.csproj      # 项目文件
├── DbcFiles/               # 资源文件
├── EasyDbc.Benchmark/      # 性能测试文件夹
├── EasyDbc.Test/           # 单元测试文件夹
├── EasyDbc.Demo/           # 示例项目文件夹
├── README.zh_CN.md         # 中文项目说明文件
├── README.md               # 项目说明文件
└── LICENSE                 # 许可证文件
```
---

# 快速入门

## 程序运行展示 Demo

以下是本项目的一些功能展示和运行效果截图。

**样例说明截图**：

![软件使用说明](Assets/DemoDescription.jpg)


---

## 功能 1: **DBC 文件解析**

通过指定路径解析 DBC 文件，并成功加载内容。

**运行效果截图**：

![DBC 文件解析](Assets/Overview.jpg)

**示例说明**：
- 输入：选择一个格式良好的 DBC 文件。
- 输出：成功解析文件内容，展示信号列表和消息信息。

---

## 功能 2: **Excel 文件解析和生成**

支持将 Excel 文件解析为 DBC 数据模型，同时支持根据用户需求生成 Excel 文件。

**运行效果截图**：

![Excel 文件解析](Assets/ExcelView.jpg)

**示例说明**：
- 功能亮点：
  - 支持自定义逻辑解析。
  - 可以灵活地生成包含下拉菜单、数据验证和样式的 Excel 文件。

---

## 功能 3: **DBC 文件合并**

将多个 DBC 文件合并为一个，解决重复信号和冲突问题。

**运行效果截图**：

![DBC 文件合并](Assets/DbcMergeView.jpg)

**示例说明**：
- 输入：多个待合并的 DBC 文件。
- 输出：生成一个合并后的完整 DBC 文件，且支持定制化合并规则。

---


## 总结

以上是项目的一些核心功能展示。如果你对某些功能感兴趣，可以参考以下章节获取更详细的使用说明。

### 添加命名空间 

```cs
//用于Dbc文件和Excel文件生成
using EasyDbc.Generators;
//用于信号的特殊解析 例如:signal.Parent.CycleTime(out var cycleTime);
using EasyDbc.Helpers;
//关联模型
using EasyDbc.Models;
//用于DBC和Excel的文件解析
using EasyDbc.Parsers;
```
---

### 基于不同格式的文件解析成Dbc 类

```cs
private bool TryParsingToFile(string path, out Dbc dbc)
{
    string extension = Path.GetExtension(path)?.ToLower();
    //基于文件路径解析DBC文件
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
    //基于文件路径解析EXCEL文件
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

### 合并DBC功能展示,可以基于自己实际需求来添加
**⚠️ 重要提示** 

目前的合并DBC功能，如果有一样的报文，那么最先添加的有效，后面的无效。

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
### **DBC文件解析**
使用静态类 `Parser`来解析DBC文件, 选择一种解析方式:
```cs
//通过文件路径解析
Dbc dbc = Parser.ParseFromPath("C:\\your_dbc_file.dbc");
//通过Stream进行文件解析
Dbc dbc = Parser.ParseFromStream(File.OpenRead("C:\\your_dbc_file.dbc")); 
//通过文本字符串来解析
Dbc dbc = Parser.Parse("a dbc as string");
```
---

### **处理DBC文件对象**

`DBC`对象包含两个集合，`Messages`和`Nodes`，他们都是`IEnumerable<T>`的类型，因此可以使用标准的LINQ进行访问、迭代和查询。

例如，获取所有ID大于100且信号数量大于2的报文:
```cs
var filteredSelection = dbc
			.Messages
			.Where(m => m.ID > 100 && m.Signals.Count > 2)
			.ToArray();
```
---

### **解析错误管理**

用于在解析过程中向用户通报发生的语法错误。
`IParseFailureObserver` 接口提供了处理语法错误的所有方法，例如:
- 通用语法错误（例如缺少 `;`、`'`、`,`）
- 重复的对象定义（例如具有相同 ID 的消息；具有相同名称的节点、信号、自定义属性等）
- 缺少对象定义（例如自定义属性在声明之前被赋值）
- 值一致性（例如自定义属性值超出属性定义中的最小值和最大值范围）

该库提供了两种不同的实现：

1. SilentFailureObserver：默认实现。它会在解析时屏蔽错误
2. SimpleFailureObserver：简单的观察者，会记录任何错误。SimpleFailureObserver 错误列表：
    - Unknown syntax：没有对应的 TAG 语法
    - [TAG] Syntax error：语法 错误，特定 TAG 的语法错误
    - Duplicated object：解析器发现（并忽略）了重复的对象
    - Object Not found：某个对象在定义之前被声明或引用
    - Property value out of bound：分配给属性的值低于/高于属性定义中的最小/最大值
    - Property value out of index：声明的索引值不可接受（对于支持通过索引访问值的属性，例如枚举值）

错误列表可以通过 GetErrorList() 方法获取。
```cs
    // 注释掉这两行代码以移除错误解析管理（错误将默默处理）
    // 你可以提供自己的 IParseFailureObserver 实现来自定义错误解析管理
    var failureObserver = new SimpleFailureObserver();
    Parser.SetParsingFailuresObserver(failureObserver);

    var dbc = Parser.ParseFromPath(filePath);
    var errors = failureObserver.GetErrorList();
```
---

### 信号的打包和解包

### **简单场景应用示例**

要打包和解包信号，可以使用静态类 `Packer`
打包/解包信号的示例：14 bits，最小值：`-61.92`，最大值：`101.91`

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

// 用于打包发送信号
ulong TxMsg = Packer.TxSignalPack(-34.3, sig);

// 这将解包接收到的信号并计算相应的物理值
double val = Packer.RxSignalUnpack(TxMsg, sig);
```

可以在CAN传输之前将多个信号打包:
```cs
ulong TxMsg = 0;
TxMsg |= Packer.TxSignalPack(value1, sig1);
TxMsg |= Packer.TxSignalPack(value2, sig2);
TxMsg |= Packer.TxSignalPack(value3, sig3);
// ...
// 发送TxMsg到CAN总线。
```

用户需要通过正确指定 `Length` 和 `StartBit` 来确保信号之间不会互相重叠。

### **复用(Multiplexing)信号应用示例**
一条消息可以包含复用数据，即布局可以根据复用器值的不同而变化。`Packer` 类无法处理复用，因此用户需要自行检查给定的消息是否确实包含该信号。

例如，考虑以下的 dbc 文件内容：
```
BO_ 568 UI_driverAssistRoadSign: 8 GTW
 SG_ UI_roadSign M : 0|8@1+ (1,0) [0|0] ""  DAS
 SG_ UI_dummyData m0 : 8|1@1+ (1,0) [0|0] "" Vector__XXX
 SG_ UI_stopSignStopLineDist m1 : 8|10@1+ (0.25,-8) [-8|247.5] "m" Vector__XXX
```

信号 `UI_dummyData` 仅在 `UI_roadSign` 值为 0 时可用，而 `UI_stopSignStopLineDist` 仅在 `UI_roadSign` 值为 1 时可用。
您可以通过调用以下方法来访问复用信息：
```cs
var multiplexingInfo = signal.MultiplexingInfo();
if(multiplexingInfo.Role == MultiplexingRole.Multiplexor)
{
	// 这是一个复用器！
}
else if(multiplexingInfo.Role == MultiplexingRole.Multiplexed)
{
	Console.WriteLine($"This signal is multiplexed and will be available when multiplexor value is {multiplexingInfo.Group}");
}
```
您也可以通过调用扩展方法来检查一条消息是否包含复用信号。
```cs
if(message.IsMultiplexed())
{
	// ...
}
```
---
# 贡献

欢迎贡献！请随时提交拉取请求（pull requests）来改进这个库。

---