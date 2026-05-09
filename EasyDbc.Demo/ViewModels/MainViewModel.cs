using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyDbc.Generators;
using EasyDbc.Helpers;
using EasyDbc.Models;
using EasyDbc.Observers;
using EasyDbc.Parsers;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Data;
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

    private ObservableCollection<EditableMessageViewModel> _messageItems = new();
    public ObservableCollection<EditableMessageViewModel> MessageItems
    {
        get => _messageItems;
        set
        {
            if (SetProperty(ref _messageItems, value))
            {
                MessageItemsView = CollectionViewSource.GetDefaultView(_messageItems);
                ApplyMessageSorting();
            }
        }
    }

    private ICollectionView _messageItemsView;
    public ICollectionView MessageItemsView
    {
        get => _messageItemsView;
        set => SetProperty(ref _messageItemsView, value);
    }

    public IReadOnlyList<string> MessageSortFields { get; } = new List<string>
    {
        "ID",
        "Name",
        "CycleTime",
        "DLC"
    };

    private string _selectedMessageSortField = "ID";
    public string SelectedMessageSortField
    {
        get => _selectedMessageSortField;
        set
        {
            if (SetProperty(ref _selectedMessageSortField, value))
            {
                ApplyMessageSorting();
            }
        }
    }

    private bool _isMessageSortDescending;
    public bool IsMessageSortDescending
    {
        get => _isMessageSortDescending;
        set
        {
            if (SetProperty(ref _isMessageSortDescending, value))
            {
                OnPropertyChanged(nameof(SortDirectionText));
                OnPropertyChanged(nameof(SortDirectionIcon));
                ApplyMessageSorting();
            }
        }
    }

    public string SortDirectionText => IsMessageSortDescending ? "Descending" : "Ascending";

    public string SortDirectionIcon => IsMessageSortDescending ? "↓" : "↑";

    private bool _areAllMessagesExpanded;
    public bool AreAllMessagesExpanded
    {
        get => _areAllMessagesExpanded;
        set
        {
            if (SetProperty(ref _areAllMessagesExpanded, value))
            {
                OnPropertyChanged(nameof(ToggleExpandText));
                OnPropertyChanged(nameof(ToggleExpandIcon));
            }
        }
    }

    public string ToggleExpandText => AreAllMessagesExpanded ? "Collapse All" : "Expand All";

    public string ToggleExpandIcon => AreAllMessagesExpanded ? "▾" : "▸";

    private ICommand _openFileCommand;
    public ICommand OpenFileCommand => _openFileCommand ??= new RelayCommand<string>(OnOpenFileCommand);

    private void OnOpenFileCommand(string obj)
    {
        if (string.IsNullOrEmpty(obj))
            return;
        var openFileDialog = new OpenFileDialog
        {
            Title = "Please select a excel or dbc file",
            Filter = "Supported Files|*.dbc;*.xls;*.xlsx",
            FilterIndex = 1,
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
            fileter = "dbc Files|*.dbc";
            fileFormat = "_DBC";
        }
        else if (obj == "OutputExcelFilePath")
        {
            fileter = "Excel Files|*.xls;*.xlsx";
            fileFormat = "_Excel";
        }
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Please select path for save",
            FileName = $"Generated{fileFormat}File_{timeString}",
            Filter = fileter,
            FilterIndex = 1,
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
        if (_mergedDbc == null && !ParsingAndMergeDbc())
        {
            MessageBox.Show("The DBC parsing result is empty. Please confirm if the file is correct. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_mergedDbc != null)
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
    }
    private ICommand _parsingMessageCommand;
    public ICommand ParsingMessageCommand => _parsingMessageCommand ??= new RelayCommand(OnParsingMessagesCommand);

    private ICommand _toggleMessageSortDirectionCommand;
    public ICommand ToggleMessageSortDirectionCommand => _toggleMessageSortDirectionCommand ??= new RelayCommand(OnToggleMessageSortDirectionCommand);

    private ICommand _toggleExpandAllMessagesCommand;
    public ICommand ToggleExpandAllMessagesCommand => _toggleExpandAllMessagesCommand ??= new RelayCommand(OnToggleExpandAllMessagesCommand);

    private void OnParsingMessagesCommand()
    {
        ParsingAndMergeDbc();
    }

    private void OnToggleMessageSortDirectionCommand()
    {
        IsMessageSortDescending = !IsMessageSortDescending;
    }

    private void OnToggleExpandAllMessagesCommand()
    {
        var shouldExpand = !AreAllMessagesExpanded;
        foreach (var messageItem in MessageItems)
        {
            messageItem.IsExpanded = shouldExpand;
        }

        AreAllMessagesExpanded = shouldExpand;
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
        MessageItems.Clear();
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
            GenerateEditableMessages(_mergedDbc);
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

    private void GenerateEditableMessages(Dbc dbc)
    {
        MessageItems = new ObservableCollection<EditableMessageViewModel>(dbc.Messages.Select(message => new EditableMessageViewModel(message)));
        AreAllMessagesExpanded = false;
        foreach (var messageItem in MessageItems)
        {
            messageItem.IsExpanded = false;
        }
    }

    private void ApplyMessageSorting()
    {
        if (MessageItemsView == null)
        {
            return;
        }

        MessageItemsView.SortDescriptions.Clear();
        MessageItemsView.SortDescriptions.Add(new SortDescription(SelectedMessageSortField, IsMessageSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
        MessageItemsView.Refresh();
    }

    public class EditableMessageViewModel : ObservableObject
    {
        private readonly Message _message;
        private readonly SilentFailureObserver _silentFailureObserver = new();

        public EditableMessageViewModel(Message message)
        {
            _message = message;
            Signals = new ObservableCollection<EditableSignalViewModel>(_message.Signals.Select(signal => new EditableSignalViewModel(signal)));
            _message.CycleTime(out _cycleTime);
            _isExpanded = false;
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public string Id
        {
            get => $"0x{_message.ID:X}";
            set
            {
                if (TryParseId(value, out var id))
                {
                    _message.ID = id;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _message.Name;
            set
            {
                if (_message.Name != value)
                {
                    _message.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _cycleTime;
        public int CycleTime
        {
            get => _cycleTime;
            set
            {
                if (SetProperty(ref _cycleTime, value))
                {
                    SetCycleTime(value);
                }
            }
        }

        public bool IsExtendedId
        {
            get => _message.IsExtID;
            set
            {
                if (_message.IsExtID != value)
                {
                    _message.IsExtID = value;
                    OnPropertyChanged();
                }
            }
        }

        public ushort Dlc
        {
            get => _message.DLC;
            set
            {
                if (_message.DLC != value)
                {
                    _message.DLC = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _message.Comment;
            set
            {
                if (_message.Comment != value)
                {
                    _message.Comment = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<EditableSignalViewModel> Signals { get; }

        private void SetCycleTime(int value)
        {
            if (_message.CustomProperties.TryGetValue("GenMsgCycleTime", out var cycleProperty) && cycleProperty.IntegerCustomProperty != null)
            {
                cycleProperty.IntegerCustomProperty.Value = value;
                return;
            }

            var definition = new CustomPropertyDefinition(_silentFailureObserver)
            {
                Name = "GenMsgCycleTime",
                DataType = CustomPropertyDataType.Integer,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Minimum = 0,
                    Maximum = 0,
                    Default = value
                }
            };

            var property = new CustomProperty(definition)
            {
                IntegerCustomProperty = new CustomPropertyValue<int> { Value = value }
            };
            _message.CustomProperties["GenMsgCycleTime"] = property;
        }

        private static bool TryParseId(string value, out uint id)
        {
            id = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.Trim();
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return uint.TryParse(value[2..], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out id);
            }

            if (uint.TryParse(value, out id))
            {
                return true;
            }

            return uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out id);
        }
    }

    public class EditableSignalViewModel : ObservableObject
    {
        private readonly Signal _signal;
        private string _valueTable;

        public EditableSignalViewModel(Signal signal)
        {
            _signal = signal;
            _valueTable = string.Join(Environment.NewLine, _signal.ValueTableMap.Select(kvp => $"{kvp.Key}\"{kvp.Value}\""));
        }

        public string Name
        {
            get => _signal.Name;
            set
            {
                if (_signal.Name != value)
                {
                    _signal.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public ushort StartBit
        {
            get => _signal.StartBit;
            set
            {
                if (_signal.StartBit != value)
                {
                    _signal.StartBit = value;
                    OnPropertyChanged();
                }
            }
        }

        public ushort Length
        {
            get => _signal.Length;
            set
            {
                if (_signal.Length != value)
                {
                    _signal.Length = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ByteOrder
        {
            get => _signal.ByteOrder == 1 ? "Intel" : "Motorola";
            set
            {
                var byteOrder = string.Equals(value, "Motorola", StringComparison.OrdinalIgnoreCase) ? (byte)0 : (byte)1;
                if (_signal.ByteOrder != byteOrder)
                {
                    _signal.ByteOrder = byteOrder;
                    OnPropertyChanged();
                }
            }
        }

        public DbcValueType ValueType
        {
            get => _signal.ValueType;
            set
            {
                if (_signal.ValueType != value)
                {
                    _signal.ValueType = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Factor
        {
            get => _signal.Factor;
            set
            {
                if (_signal.Factor != value)
                {
                    _signal.Factor = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Offset
        {
            get => _signal.Offset;
            set
            {
                if (_signal.Offset != value)
                {
                    _signal.Offset = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Minimum
        {
            get => _signal.Minimum;
            set
            {
                if (_signal.Minimum != value)
                {
                    _signal.Minimum = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Maximum
        {
            get => _signal.Maximum;
            set
            {
                if (_signal.Maximum != value)
                {
                    _signal.Maximum = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Unit
        {
            get => _signal.Unit;
            set
            {
                if (_signal.Unit != value)
                {
                    _signal.Unit = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ValueTable
        {
            get => _valueTable;
            set => SetProperty(ref _valueTable, value);
        }

        public string Comment
        {
            get => _signal.Comment;
            set
            {
                if (_signal.Comment != value)
                {
                    _signal.Comment = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
