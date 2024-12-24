using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyDbc.Generators;
using EasyDbc.Helpers;
using EasyDbc.Models;
using EasyDbc.Parsers;
using Microsoft.Win32;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace EasyDbc.Demo.ViewModels;

public class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
    }
    private Dbc _mergedDbc = null;
    //Input File Path
    private string _filePath1;
    public string FilePath1
    {
        get { return _filePath1; }
        set { SetProperty(ref _filePath1, value); }
    }
    private string _filePath2;
    public string FilePath2
    {
        get { return _filePath2; }
        set { SetProperty(ref _filePath2, value); }
    }
    private string _filePath3;
    public string FilePath3
    {
        get { return _filePath3; }
        set { SetProperty(ref _filePath3, value); }
    }
    //Output File Path
    private string _outputDbcFilePath;
    public string OutputDbcFilePath
    {
        get { return _outputDbcFilePath; }
        set { SetProperty(ref _outputDbcFilePath, value); }
    }
    private string _outputExcelFilePath;
    public string OutputExcelFilePath
    {
        get { return _outputExcelFilePath; }
        set { SetProperty(ref _outputExcelFilePath, value); }
    }
    private string _nodes = string.Empty;
    public string Nodes
    {
        get { return _nodes; }
        set { SetProperty(ref _nodes, value); }
    }
    private DataTable _messages = new DataTable();
    public DataTable Messages
    {
        get { return _messages; }
        set { SetProperty(ref _messages, value); }
    }

    private ICommand _openFileCommand;
    public ICommand OpenFileCommand => _openFileCommand ??= new RelayCommand<string>(OnOpenFileCommand);

    private void OnOpenFileCommand(string obj)
    {
        if (string.IsNullOrEmpty(obj))
            return;
        var openFileDialog = new OpenFileDialog
        {
            Title = "Please select a excel or dbc file",
            Filter = "All Files|*.*|Excel Files|*.xls;*.xlsx|dbc Files|*.dbc",
            FilterIndex = 2,
            Multiselect = false,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            RestoreDirectory = true,
        };
        if (openFileDialog.ShowDialog() == true)
        {
            //Extension validation
            if (IsValidExtension(openFileDialog.FileName))
            {
                if (string.Equals(obj, "FilePath1", StringComparison.OrdinalIgnoreCase))
                {
                    FilePath1 = openFileDialog.FileName;
                }
                else if (string.Equals(obj, "FilePath2", StringComparison.OrdinalIgnoreCase))
                {
                    FilePath2 = openFileDialog.FileName;
                }
                else if (string.Equals(obj, "FilePath3", StringComparison.OrdinalIgnoreCase))
                {
                    FilePath3 = openFileDialog.FileName;
                }
            }
            else
            {
                MessageBox.Show("Invalid file extesion", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

    }
    private ICommand clearFileCommand;
    public ICommand ClearFileCommand => clearFileCommand ??= new RelayCommand<string>(OnClearFilePathCommand);

    private void OnClearFilePathCommand(string obj)
    {
        if (string.Equals(obj, "FilePath1", StringComparison.OrdinalIgnoreCase))
        {
            FilePath1 = string.Empty;
        }
        else if (string.Equals(obj, "FilePath2", StringComparison.OrdinalIgnoreCase))
        {
            FilePath2 = string.Empty;
        }
        else if (string.Equals(obj, "FilePath3", StringComparison.OrdinalIgnoreCase))
        {
            FilePath3 = string.Empty;
        }
    }
    private ICommand _saveFileCommand;
    public ICommand SaveFileCommand => _saveFileCommand ??= new RelayCommand<string>(OnSaveFileCommand);

    private void OnSaveFileCommand(string obj)
    {
        string timeString = DateTime.Now.ToString("yyyyMMdd_HH_mm_ss");
        if (string.IsNullOrEmpty(obj))
        {
            return;
        }
        string fileter = "All Files|*.*";
        string fileFormat = string.Empty;
        // Add file extension fileter 
        if (obj == "OutputDbcFilePath")
        {
            fileter = "All Files|*.*|dbc Files|*.dbc";
            fileFormat = "_DBC";
        }
        else if (obj == "OutputExcelFilePath")
        {
            fileter = "All Files|*.*|Excel Files 2003|*.xls|Excel Files|*.xlsx";
            fileFormat = "_Excel";
        }
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Please select path for save",
            FileName = $"Generated{fileFormat}File_{timeString}",
            Filter = fileter,
            FilterIndex = 3,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            RestoreDirectory = true,
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            // Extension validation
            string extension = Path.GetExtension(saveFileDialog.FileName)?.ToLower();
            if (extension == ".dbc" && obj == "OutputDbcFilePath")
            {
                OutputDbcFilePath = saveFileDialog.FileName;
            }
            else if ((extension == ".xls" || extension == ".xlsx") && obj == "OutputExcelFilePath")
            {
                OutputExcelFilePath = saveFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("Invalid file extesion", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private ICommand _generateFileCommand;
    public ICommand GenerateFileCommand => _generateFileCommand ??= new RelayCommand<string>(OnGenerateFileCommand);

    private void OnGenerateFileCommand(string obj)
    {
        if (ParsingAndMergeDbc())
        {
            if (obj == "dbc")
            {
                DbcGenerator.WriteToFile(_mergedDbc, OutputDbcFilePath);
                if (File.Exists(OutputDbcFilePath))
                {
                    MessageBoxResult result = MessageBox.Show("Do you need to navigate to the file generation path?", "File generated successfully", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", Path.GetDirectoryName(OutputDbcFilePath));
                    }
                }
            }
            else if (obj == "excel")
            {
                ExcelGenerator excelGenerator = new ExcelGenerator();
                WriteStatus status = excelGenerator.WriteToFile(_mergedDbc, OutputExcelFilePath, "CanMatrixSheet");
                if (status == WriteStatus.Success)
                {
                    MessageBoxResult result = MessageBox.Show("Do you need to navigate to the file generation path?", "File generated successfully", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", Path.GetDirectoryName(OutputDbcFilePath));
                    }
                }

            }
        }
        else
        {
            MessageBox.Show("The DBC parsing result is empty. Please confirm if the file is correct. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private ICommand _parsingMessageCommand;
    public ICommand ParsingMessageCommand => _parsingMessageCommand ??= new RelayCommand(OnParsingMessagesCommand);

    private void OnParsingMessagesCommand()
    {
        ParsingAndMergeDbc();
    }

    private bool IsValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        string extension = Path.GetExtension(fileName)?.ToLower();

        return extension == ".xls" || extension == ".xlsx" || extension == ".dbc";
    }
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
            GenerateDataTable(_mergedDbc);
        }
        return result;
    }
    private bool TryParsingToFile(string path, out Dbc dbc)
    {
        string extension = Path.GetExtension(path)?.ToLower();
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
    private void GenerateDataTable(Dbc dbc)
    {
        _messages.Clear();
        _messages.Columns.Clear();
        _messages.Columns.Add("ID");
        _messages.Columns.Add("Message Name");
        _messages.Columns.Add("DLC");
        _messages.Columns.Add("Transmitter");
        _messages.Columns.Add("CycleTime");
        _messages.Columns.Add("Signal Name");
        _messages.Columns.Add("Start Bit");
        _messages.Columns.Add("Length");
        _messages.Columns.Add("Byte Order");
        _messages.Columns.Add("Data Type");
        _messages.Columns.Add("Factor");
        _messages.Columns.Add("Offset");
        _messages.Columns.Add("Minimum");
        _messages.Columns.Add("Maximum");
        _messages.Columns.Add("Initial Value");
        _messages.Columns.Add("Unit");
        _messages.Columns.Add("ValueTable");
        _messages.Columns.Add("Comment");
        foreach (Message message in dbc.Messages)
        {
            foreach (Signal signal in message.Signals)
            {
                signal.Parent.CycleTime(out var cycleTime);
                var valueTableString = string.Join("\n", signal.ValueTableMap);
                _messages.Rows.Add($"0x{signal.Parent.ID.ToString("X")}",
                    signal.Parent.Name,
                    signal.Parent.DLC,
                    signal.Parent.Transmitter,
                    cycleTime,
                    signal.Name,
                    signal.StartBit,
                    signal.Length,
                    signal.ByteOrder == 1 ? "Intel" : "Motorola",
                    signal.ValueType,
                    signal.Factor,
                    signal.Offset,
                    signal.Minimum,
                    signal.Maximum,
                    signal.InitialValue,
                    signal.Unit,
                    valueTableString,
                    signal.Comment
                    );
            }
        }
        Messages = _messages;
    }
}
