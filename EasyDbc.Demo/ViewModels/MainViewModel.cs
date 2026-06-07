using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyDbc.Generators;
using EasyDbc.Helpers;
using EasyDbc.Models;
using EasyDbc.Observers;
using EasyDbc.Parsers;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyDbc.Demo.ViewModels;

public class MainViewModel : ObservableObject
{
    private const int MaxNameLength = 32;
    private const int MaxRecentPaths = 10;
    public MainViewModel()
    {
        LoadSettings();
    }
    private Dbc _mergedDbc = null;
    private readonly Stack<DeletedMessageEntry> _deletedMessages = new();
    private readonly AppSettings _settings = new();
    private bool _suppressSettingsSave;
    private const string SettingsFileName = "EasyDbcSettings.json";
    private static readonly Brush[] DuplicateIdPalette = new[]
    {
        new SolidColorBrush(Color.FromRgb(239, 83, 80)),
        new SolidColorBrush(Color.FromRgb(229, 57, 53)),
        new SolidColorBrush(Color.FromRgb(216, 27, 96)),
        new SolidColorBrush(Color.FromRgb(194, 24, 91)),
        new SolidColorBrush(Color.FromRgb(142, 36, 170)),
        new SolidColorBrush(Color.FromRgb(123, 31, 162)),
        new SolidColorBrush(Color.FromRgb(106, 27, 154)),
        new SolidColorBrush(Color.FromRgb(94, 53, 177)),
        new SolidColorBrush(Color.FromRgb(74, 20, 140)),
        new SolidColorBrush(Color.FromRgb(171, 71, 188)),
        new SolidColorBrush(Color.FromRgb(149, 117, 205)),
        new SolidColorBrush(Color.FromRgb(186, 104, 200)),
        new SolidColorBrush(Color.FromRgb(255, 82, 82)),
        new SolidColorBrush(Color.FromRgb(255, 23, 68)),
        new SolidColorBrush(Color.FromRgb(213, 0, 0)),
        new SolidColorBrush(Color.FromRgb(198, 40, 40)),
        new SolidColorBrush(Color.FromRgb(183, 28, 28)),
        new SolidColorBrush(Color.FromRgb(128, 0, 128)),
        new SolidColorBrush(Color.FromRgb(147, 0, 211)),
        new SolidColorBrush(Color.FromRgb(199, 21, 133)),
    };
    //Input File Path
    private string _filePath1;
    public string FilePath1
    {
        get { return _filePath1; }
        set
        {
            if (SetProperty(ref _filePath1, value))
            {
                SaveSettings();
            }
        }
    }
    private string _filePath2;
    public string FilePath2
    {
        get { return _filePath2; }
        set
        {
            if (SetProperty(ref _filePath2, value))
            {
                SaveSettings();
            }
        }
    }
    private string _filePath3;
    public string FilePath3
    {
        get { return _filePath3; }
        set
        {
            if (SetProperty(ref _filePath3, value))
            {
                SaveSettings();
            }
        }
    }
    //Output File Path
    private string _outputDbcFilePath;
    public string OutputDbcFilePath
    {
        get { return _outputDbcFilePath; }
        set
        {
            if (SetProperty(ref _outputDbcFilePath, value))
            {
                SaveSettings();
            }
        }
    }
    private string _outputExcelFilePath;
    public string OutputExcelFilePath
    {
        get { return _outputExcelFilePath; }
        set
        {
            if (SetProperty(ref _outputExcelFilePath, value))
            {
                SaveSettings();
            }
        }
    }

    public ObservableCollection<string> RecentInputFiles { get; } = new();

    public ObservableCollection<string> RecentDbcOutputFiles { get; } = new();

    public ObservableCollection<string> RecentExcelOutputFiles { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _parsingMessageCommand?.NotifyCanExecuteChanged();
                _generateFileCommand?.NotifyCanExecuteChanged();
            }
        }
    }

    private bool _includeSignalGroupExcelColumns = true;
    public bool IncludeSignalGroupExcelColumns
    {
        get => _includeSignalGroupExcelColumns;
        set
        {
            if (SetProperty(ref _includeSignalGroupExcelColumns, value))
            {
                SaveSettings();
            }
        }
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
            var previousItems = _messageItems;
            if (SetProperty(ref _messageItems, value))
            {
                DetachMessageItemHandlers(previousItems);
                MessageItemsView = CollectionViewSource.GetDefaultView(_messageItems);
                ApplyMessageSorting();
                AttachMessageItemHandlers(_messageItems);
                UpdateMessageHighlighting();
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

    private RelayCommand<EditableMessageViewModel> _deleteMessageCommand;
    public ICommand DeleteMessageCommand => _deleteMessageCommand ??= new RelayCommand<EditableMessageViewModel>(OnDeleteMessage, CanDeleteMessage);

    private RelayCommand _undoDeleteMessageCommand;
    public ICommand UndoDeleteMessageCommand => _undoDeleteMessageCommand ??= new RelayCommand(OnUndoDeleteMessage, CanUndoDeleteMessage);

    private RelayCommand<EditableSignalViewModel> _deleteSignalCommand;
    public ICommand DeleteSignalCommand => _deleteSignalCommand ??= new RelayCommand<EditableSignalViewModel>(OnDeleteSignal, CanDeleteSignal);

    private ICommand _openFileCommand;
    public ICommand OpenFileCommand => _openFileCommand ??= new RelayCommand<string>(OnOpenFileCommand);

    private void OnOpenFileCommand(string obj)
    {
        if (string.IsNullOrEmpty(obj))
            return;
        var initialDirectory = GetInitialOpenDirectory(obj);
        var openFileDialog = new OpenFileDialog
        {
            Title = "Please select a excel or dbc file",
            Filter = "Supported Files|*.dbc;*.xls;*.xlsx",
            FilterIndex = 1,
            Multiselect = false,
            InitialDirectory = initialDirectory,
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
                AddRecentPath(RecentInputFiles, openFileDialog.FileName);
                _settings.LastOpenDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                SaveSettings();
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
            InitialDirectory = GetInitialSaveDirectory(obj),
            RestoreDirectory = true,
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            // Extension validation
            string extension = Path.GetExtension(saveFileDialog.FileName)?.ToLower();
            if (extension == ".dbc" && obj == "OutputDbcFilePath")
            {
                OutputDbcFilePath = saveFileDialog.FileName;
                AddRecentPath(RecentDbcOutputFiles, saveFileDialog.FileName);
            }
            else if ((extension == ".xls" || extension == ".xlsx") && obj == "OutputExcelFilePath")
            {
                OutputExcelFilePath = saveFileDialog.FileName;
                AddRecentPath(RecentExcelOutputFiles, saveFileDialog.FileName);
            }
            else
            {
                MessageBox.Show("Invalid file extesion", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _settings.LastSaveDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
            SaveSettings();
        }
    }

    private AsyncRelayCommand<string> _generateFileCommand;
    public ICommand GenerateFileCommand => _generateFileCommand ??= new AsyncRelayCommand<string>(OnGenerateFileCommandAsync, CanRunLongOperation);

    private async Task OnGenerateFileCommandAsync(string obj)
    {
        if (_mergedDbc == null && !await ParsingAndMergeDbcAsync())
        {
            MessageBox.Show("The DBC parsing result is empty. Please confirm if the file is correct. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_mergedDbc != null)
        {
            if (obj == "dbc")
            {
                try
                {
                    IsBusy = true;
                    await Task.Run(() => DbcGenerator.WriteToFile(_mergedDbc, OutputDbcFilePath));
                }
                finally
                {
                    IsBusy = false;
                }
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
                WriteStatus status;
                try
                {
                    IsBusy = true;
                    status = await Task.Run(() =>
                    {
                        ExcelGenerator excelGenerator = new ExcelGenerator { IncludeSignalGroupColumns = IncludeSignalGroupExcelColumns };
                        return excelGenerator.WriteToFile(_mergedDbc, OutputExcelFilePath, "CanMatrixSheet");
                    });
                }
                finally
                {
                    IsBusy = false;
                }
                if (status == WriteStatus.Success)
                {
                    MessageBoxResult result = MessageBox.Show("Do you need to navigate to the file generation path?", "File generated successfully", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", Path.GetDirectoryName(OutputExcelFilePath));
                    }
                }

            }
        }
    }
    private AsyncRelayCommand _parsingMessageCommand;
    public ICommand ParsingMessageCommand => _parsingMessageCommand ??= new AsyncRelayCommand(OnParsingMessagesCommandAsync, CanRunLongOperation);

    private ICommand _toggleMessageSortDirectionCommand;
    public ICommand ToggleMessageSortDirectionCommand => _toggleMessageSortDirectionCommand ??= new RelayCommand(OnToggleMessageSortDirectionCommand);

    private ICommand _toggleExpandAllMessagesCommand;
    public ICommand ToggleExpandAllMessagesCommand => _toggleExpandAllMessagesCommand ??= new RelayCommand(OnToggleExpandAllMessagesCommand);

    private async Task OnParsingMessagesCommandAsync()
    {
        await ParsingAndMergeDbcAsync();
    }

    private bool CanRunLongOperation()
    {
        return !IsBusy;
    }

    private bool CanRunLongOperation(string _)
    {
        return !IsBusy;
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
    private async Task<bool> ParsingAndMergeDbcAsync()
    {
        IsBusy = true;
        try
        {
            var result = await Task.Run(BuildParseDisplayResult);
            Nodes = string.Empty;
            Messages.Clear();
            MessageItems.Clear();
            _mergedDbc = null;

            if (result == null || result.MergedDbc == null)
            {
                return false;
            }

            _mergedDbc = result.MergedDbc;
            Nodes = result.Nodes;
            Messages = result.Messages;
            MessageItems = result.MessageItems;
            AreAllMessagesExpanded = false;
            foreach (var messageItem in MessageItems)
            {
                messageItem.IsExpanded = false;
            }
            _deletedMessages.Clear();
            _undoDeleteMessageCommand?.NotifyCanExecuteChanged();
            UpdateMessageHighlighting();
            return true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private ParseDisplayResult BuildParseDisplayResult()
    {
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
        bool result = DbcGenerator.MergeDbc(parsingResult, out var mergedDbc);
        if (result)
        {
            return new ParseDisplayResult
            {
                MergedDbc = mergedDbc,
                Nodes = string.Join("; ", mergedDbc.Nodes.Select(node => node.Name)),
                Messages = BuildMessagesDataTable(mergedDbc),
                MessageItems = BuildEditableMessageItems(mergedDbc),
            };
        }
        return null;
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
            excelParser.ParseSignalGroupColumns = IncludeSignalGroupExcelColumns;
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
    private static DataTable BuildMessagesDataTable(Dbc dbc)
    {
        var messages = new DataTable();
        messages.Columns.Add("ID");
        messages.Columns.Add("Message Name");
        messages.Columns.Add("DLC");
        messages.Columns.Add("Transmitter");
        messages.Columns.Add("CycleTime");
        messages.Columns.Add("Signal Name");
        messages.Columns.Add("Start Bit");
        messages.Columns.Add("Length");
        messages.Columns.Add("Byte Order");
        messages.Columns.Add("Data Type");
        messages.Columns.Add("Factor");
        messages.Columns.Add("Offset");
        messages.Columns.Add("Minimum");
        messages.Columns.Add("Maximum");
        messages.Columns.Add("Initial Value");
        messages.Columns.Add("Unit");
        messages.Columns.Add("ValueTable");
        messages.Columns.Add("Multiplexing");
        messages.Columns.Add("MultiplexRanges");
        messages.Columns.Add("SignalGroups");
        messages.Columns.Add("Comment");
        foreach (Message message in dbc.Messages)
        {
            foreach (Signal signal in message.Signals)
            {
                signal.Parent.CycleTime(out var cycleTime);
                var valueTableString = string.Join("\n", signal.ValueTableMap);
                var multiplexRangesString = FormatMultiplexRanges(signal.MultiplexRanges);
                var signalGroupsString = FormatSignalGroups(signal);
                messages.Rows.Add($"0x{signal.Parent.ID.ToString("X")}",
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
                    signal.Multiplexing,
                    multiplexRangesString,
                    signalGroupsString,
                    signal.Comment
                    );
            }
        }
        return messages;
    }

    private void GenerateEditableMessages(Dbc dbc)
    {
        MessageItems = BuildEditableMessageItems(dbc);
        AreAllMessagesExpanded = false;
        foreach (var messageItem in MessageItems)
        {
            messageItem.IsExpanded = false;
        }
        _deletedMessages.Clear();
        _undoDeleteMessageCommand?.NotifyCanExecuteChanged();
        UpdateMessageHighlighting();
    }

    private static ObservableCollection<EditableMessageViewModel> BuildEditableMessageItems(Dbc dbc)
    {
        return new ObservableCollection<EditableMessageViewModel>(dbc.Messages.Select(message => new EditableMessageViewModel(message)));
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

    private void AttachMessageItemHandlers(ObservableCollection<EditableMessageViewModel> items)
    {
        if (items == null)
        {
            return;
        }

        items.CollectionChanged += OnMessageItemsCollectionChanged;
        foreach (var item in items)
        {
            item.PropertyChanged += OnMessageItemPropertyChanged;
        }
    }

    private void DetachMessageItemHandlers(ObservableCollection<EditableMessageViewModel> items)
    {
        if (items == null)
        {
            return;
        }

        items.CollectionChanged -= OnMessageItemsCollectionChanged;
        foreach (var item in items)
        {
            item.PropertyChanged -= OnMessageItemPropertyChanged;
        }
    }

    private void OnMessageItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (EditableMessageViewModel item in e.OldItems)
            {
                item.PropertyChanged -= OnMessageItemPropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (EditableMessageViewModel item in e.NewItems)
            {
                item.PropertyChanged += OnMessageItemPropertyChanged;
            }
        }

        UpdateMessageHighlighting();
    }

    private void OnMessageItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditableMessageViewModel.Id) || e.PropertyName == nameof(EditableMessageViewModel.Name))
        {
            UpdateMessageHighlighting();
        }
    }

    private void UpdateMessageHighlighting()
    {
        if (MessageItems == null || MessageItems.Count == 0)
        {
            return;
        }

        var duplicateIdGroups = MessageItems
            .GroupBy(item => item.MessageIdValue)
            .Where(group => group.Count() > 1)
            .OrderBy(group => group.Key)
            .ToList();

        var duplicateIdPaletteMap = new Dictionary<uint, Brush>();
        for (var i = 0; i < duplicateIdGroups.Count; i++)
        {
            var colorIndex = i % DuplicateIdPalette.Length;
            duplicateIdPaletteMap[duplicateIdGroups[i].Key] = DuplicateIdPalette[colorIndex];
        }

        var nameCounts = MessageItems
            .GroupBy(item => item.MessageNameValue ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        foreach (var item in MessageItems)
        {
            if (duplicateIdPaletteMap.TryGetValue(item.MessageIdValue, out var duplicateBrush))
            {
                item.HasDuplicateId = true;
                item.DuplicateIdBrush = duplicateBrush;
            }
            else
            {
                item.HasDuplicateId = false;
                item.DuplicateIdBrush = Brushes.Transparent;
            }
            var nameKey = item.MessageNameValue ?? string.Empty;
            item.HasDuplicateName = nameCounts.TryGetValue(nameKey, out var nameCount) && nameCount > 1;
            item.UpdateHasNoSignals();
        }
    }

    private bool CanDeleteMessage(EditableMessageViewModel message)
    {
        return message != null;
    }

    private void OnDeleteMessage(EditableMessageViewModel message)
    {
        if (message == null)
        {
            return;
        }

        var viewIndex = MessageItems.IndexOf(message);
        if (viewIndex < 0)
        {
            return;
        }

        var messageList = _mergedDbc?.Messages as IList<Message>;
        var modelIndex = messageList?.IndexOf(message.SourceMessage) ?? -1;
        var detachedSignals = message.SourceMessage.Signals.ToList();
        foreach (var signal in detachedSignals)
        {
            signal.Parent = null;
        }
        message.SourceMessage.Signals.Clear();

        MessageItems.RemoveAt(viewIndex);
        if (messageList != null && modelIndex >= 0)
        {
            messageList.RemoveAt(modelIndex);
        }

        _deletedMessages.Push(new DeletedMessageEntry(message, viewIndex, modelIndex, detachedSignals));
        _undoDeleteMessageCommand?.NotifyCanExecuteChanged();
        UpdateMessageHighlighting();
        MessageItemsView?.Refresh();
    }

    private bool CanUndoDeleteMessage()
    {
        return _deletedMessages.Count > 0;
    }

    private bool CanDeleteSignal(EditableSignalViewModel signal)
    {
        return signal?.ParentMessage != null;
    }

    private void OnDeleteSignal(EditableSignalViewModel signal)
    {
        if (signal?.ParentMessage == null)
        {
            return;
        }

        var parentMessage = signal.ParentMessage;
        var sourceMessage = parentMessage.SourceMessage;
        var sourceSignal = signal.SourceSignal;

        if (sourceSignal != null)
        {
            sourceSignal.Parent = null;
        }

        parentMessage.Signals.Remove(signal);
        sourceMessage?.Signals.Remove(sourceSignal);
        parentMessage.UpdateHasNoSignals();
        UpdateMessageHighlighting();
        MessageItemsView?.Refresh();
    }

    private void OnUndoDeleteMessage()
    {
        if (_deletedMessages.Count == 0)
        {
            return;
        }

        var entry = _deletedMessages.Pop();
        var insertIndex = entry.ViewIndex >= 0 && entry.ViewIndex <= MessageItems.Count
            ? entry.ViewIndex
            : MessageItems.Count;

        MessageItems.Insert(insertIndex, entry.MessageItem);

        var messageList = _mergedDbc?.Messages as IList<Message>;
        if (messageList != null)
        {
            var modelIndex = entry.ModelIndex >= 0 && entry.ModelIndex <= messageList.Count
                ? entry.ModelIndex
                : messageList.Count;
            messageList.Insert(modelIndex, entry.MessageItem.SourceMessage);
        }

        if (entry.DetachedSignals.Count > 0)
        {
            foreach (var signal in entry.DetachedSignals)
            {
                signal.Parent = entry.MessageItem.SourceMessage;
            }
            entry.MessageItem.SourceMessage.Signals.AddRange(entry.DetachedSignals);
        }

        _undoDeleteMessageCommand?.NotifyCanExecuteChanged();
        UpdateMessageHighlighting();
        MessageItemsView?.Refresh();
    }

    private sealed class DeletedMessageEntry
    {
        public DeletedMessageEntry(EditableMessageViewModel messageItem, int viewIndex, int modelIndex, List<Signal> detachedSignals)
        {
            MessageItem = messageItem;
            ViewIndex = viewIndex;
            ModelIndex = modelIndex;
            DetachedSignals = detachedSignals ?? new List<Signal>();
        }

        public EditableMessageViewModel MessageItem { get; }
        public int ViewIndex { get; }
        public int ModelIndex { get; }
        public List<Signal> DetachedSignals { get; }
    }

    private sealed class ParseDisplayResult
    {
        public Dbc MergedDbc { get; set; }
        public string Nodes { get; set; }
        public DataTable Messages { get; set; }
        public ObservableCollection<EditableMessageViewModel> MessageItems { get; set; }
    }

    public class EditableMessageViewModel : ObservableObject
    {
        private readonly Message _message;
        private readonly SilentFailureObserver _silentFailureObserver = new();
        private bool _hasDuplicateId;
        private bool _hasDuplicateName;
        private bool _hasNoSignals;
        private Brush _duplicateIdBrush = Brushes.Transparent;
        private bool _isSelected;
        private bool _hasSignalNameTooLong;
        private string _signalNameLengthTooltip;

        public EditableMessageViewModel(Message message)
        {
            _message = message;
            Signals = new ObservableCollection<EditableSignalViewModel>(_message.Signals.Select(signal => new EditableSignalViewModel(signal, this)));
            _message.CycleTime(out _cycleTime);
            _isExpanded = false;
            Signals.CollectionChanged += OnSignalsCollectionChanged;
            foreach (var signal in Signals)
            {
                signal.PropertyChanged += OnSignalPropertyChanged;
            }
            UpdateHasNoSignals();
            UpdateSignalNameLengthState();
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

        public Message SourceMessage => _message;

        public uint MessageIdValue => _message.ID;

        public string MessageNameValue => _message.Name;

        public bool IsMessageItem => true;

        public bool HasDuplicateId
        {
            get => _hasDuplicateId;
            set => SetProperty(ref _hasDuplicateId, value);
        }

        public Brush DuplicateIdBrush
        {
            get => _duplicateIdBrush;
            set => SetProperty(ref _duplicateIdBrush, value);
        }

        public bool HasDuplicateName
        {
            get => _hasDuplicateName;
            set => SetProperty(ref _hasDuplicateName, value);
        }

        public bool HasNoSignals
        {
            get => _hasNoSignals;
            set => SetProperty(ref _hasNoSignals, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool HasSignalNameTooLong
        {
            get => _hasSignalNameTooLong;
            set => SetProperty(ref _hasSignalNameTooLong, value);
        }

        public string SignalNameLengthTooltip
        {
            get => _signalNameLengthTooltip;
            set => SetProperty(ref _signalNameLengthTooltip, value);
        }

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

        private void OnSignalsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (EditableSignalViewModel signal in e.OldItems)
                {
                    signal.PropertyChanged -= OnSignalPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (EditableSignalViewModel signal in e.NewItems)
                {
                    signal.PropertyChanged += OnSignalPropertyChanged;
                }
            }

            UpdateHasNoSignals();
            UpdateSignalNameLengthState();
        }

        private void OnSignalPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditableSignalViewModel.IsNameTooLong) || e.PropertyName == nameof(EditableSignalViewModel.Name))
            {
                UpdateSignalNameLengthState();
            }
        }

        public void UpdateHasNoSignals()
        {
            HasNoSignals = Signals.Count == 0;
        }

        private void UpdateSignalNameLengthState()
        {
            var tooLongSignals = Signals
                .Where(signal => signal.IsNameTooLong)
                .Select(signal => signal.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            HasSignalNameTooLong = tooLongSignals.Count > 0;
            SignalNameLengthTooltip = HasSignalNameTooLong
                ? $"Signal names exceed {MaxNameLength} characters: {string.Join(", ", tooLongSignals.Take(3))}"
                : string.Empty;
        }
    }

    public class EditableSignalViewModel : ObservableObject
    {
        private readonly Signal _signal;
        private string _valueTable;
        private readonly EditableMessageViewModel _parentMessage;
        private bool _isNameTooLong;
        private string _nameLengthTooltip;

        public EditableSignalViewModel(Signal signal, EditableMessageViewModel parentMessage)
        {
            _signal = signal;
            _parentMessage = parentMessage;
            _valueTable = string.Join(Environment.NewLine, _signal.ValueTableMap.Select(kvp => $"{kvp.Key}\"{kvp.Value}\""));
            UpdateNameLengthState();
        }

        public bool IsMessageItem => false;

        public EditableMessageViewModel ParentMessage => _parentMessage;

        public Signal SourceSignal => _signal;

        public string Name
        {
            get => _signal.Name;
            set
            {
                if (_signal.Name != value)
                {
                    _signal.Name = value;
                    OnPropertyChanged();
                    UpdateNameLengthState();
                }
            }
        }
        public bool IsNameTooLong
        {
            get => _isNameTooLong;
            set => SetProperty(ref _isNameTooLong, value);
        }

        public string NameLengthTooltip
        {
            get => _nameLengthTooltip;
            set => SetProperty(ref _nameLengthTooltip, value);
        }

        private void UpdateNameLengthState()
        {
            var name = _signal.Name ?? string.Empty;
            IsNameTooLong = name.Length > MaxNameLength;
            NameLengthTooltip = IsNameTooLong
                ? $"Signal name exceeds {MaxNameLength} characters: {name}"
                : string.Empty;
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

        public string Multiplexing
        {
            get => _signal.Multiplexing;
            set
            {
                if (_signal.Multiplexing != value)
                {
                    _signal.Multiplexing = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MultiplexRanges
        {
            get => FormatMultiplexRanges(_signal.MultiplexRanges);
            set
            {
                _signal.MultiplexRanges = ParseMultiplexRanges(value);
                OnPropertyChanged();
            }
        }

        public string SignalGroups
        {
            get => FormatSignalGroups(_signal);
            set
            {
                UpdateSignalGroups(_parentMessage.SourceMessage, _signal.Name, value);
                OnPropertyChanged();
            }
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

    private static string FormatMultiplexRanges(IEnumerable<SignalMultiplexRange> multiplexRanges)
    {
        if (multiplexRanges == null)
        {
            return string.Empty;
        }

        return string.Join("; ", multiplexRanges
            .Where(range => !string.IsNullOrWhiteSpace(range.MultiplexorSignalName) && range.Ranges != null && range.Ranges.Count > 0)
            .Select(range => $"{range.MultiplexorSignalName}:{string.Join(",", range.Ranges.Select(item => $"{item.From}-{item.To}"))}"));
    }

    private static List<SignalMultiplexRange> ParseMultiplexRanges(string text)
    {
        var multiplexRanges = new List<SignalMultiplexRange>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return multiplexRanges;
        }

        var entries = text.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var parts = entry.Split(new[] { ':' }, 2);
            if (parts.Length != 2)
            {
                continue;
            }

            var ranges = new List<MultiplexRange>();
            foreach (var rangeText in parts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var bounds = rangeText.Trim().Split(new[] { '-' }, 2);
                if (bounds.Length == 2 &&
                    int.TryParse(bounds[0].Trim(), out var from) &&
                    int.TryParse(bounds[1].Trim(), out var to))
                {
                    ranges.Add(new MultiplexRange(from, to));
                }
            }

            if (ranges.Count > 0)
            {
                multiplexRanges.Add(new SignalMultiplexRange(parts[0].Trim(), ranges));
            }
        }

        return multiplexRanges;
    }

    private static string FormatSignalGroups(Signal signal)
    {
        if (signal.Parent?.SignalGroups == null)
        {
            return string.Empty;
        }

        return string.Join("; ", signal.Parent.SignalGroups
            .Where(group => group.SignalNames != null && group.SignalNames.Contains(signal.Name))
            .Select(group => group.Repetitions == 1 ? group.Name : $"{group.Name}({group.Repetitions})"));
    }

    private static void UpdateSignalGroups(Message message, string signalName, string groupText)
    {
        if (message == null || string.IsNullOrWhiteSpace(signalName))
        {
            return;
        }

        foreach (var group in message.SignalGroups)
        {
            group.SignalNames = group.SignalNames.Where(name => name != signalName).ToArray();
        }

        message.SignalGroups.RemoveAll(group => group.SignalNames.Length == 0);

        if (string.IsNullOrWhiteSpace(groupText))
        {
            return;
        }

        var groups = groupText.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var group in groups)
        {
            var groupName = group.Trim();
            var repetitions = 1;
            var match = System.Text.RegularExpressions.Regex.Match(groupName, @"^(?<Name>[\w]+)\((?<Repetitions>\d+)\)$");
            if (match.Success)
            {
                groupName = match.Groups["Name"].Value;
                int.TryParse(match.Groups["Repetitions"].Value, out repetitions);
            }

            var existingGroup = message.SignalGroups.FirstOrDefault(item => item.Name == groupName);
            if (existingGroup == null)
            {
                message.SignalGroups.Add(new SignalGroup(groupName, repetitions, new[] { signalName }));
            }
            else if (existingGroup.SignalNames.Contains(signalName) == false)
            {
                existingGroup.SignalNames = existingGroup.SignalNames.Concat(new[] { signalName }).ToArray();
            }
        }
    }

    private void LoadSettings()
    {
        var settingsPath = GetSettingsPath();
        if (!File.Exists(settingsPath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(settingsPath);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json);
            if (loaded == null)
            {
                return;
            }

            _suppressSettingsSave = true;
            _settings.LastOpenDirectory = loaded.LastOpenDirectory;
            _settings.LastSaveDirectory = loaded.LastSaveDirectory;
            FilePath1 = loaded.FilePath1;
            FilePath2 = loaded.FilePath2;
            FilePath3 = loaded.FilePath3;
            OutputDbcFilePath = loaded.OutputDbcFilePath;
            OutputExcelFilePath = loaded.OutputExcelFilePath;
            IncludeSignalGroupExcelColumns = loaded.IncludeSignalGroupExcelColumns;
            SetRecentPaths(RecentInputFiles, loaded.RecentInputFiles);
            SetRecentPaths(RecentDbcOutputFiles, loaded.RecentDbcOutputFiles);
            SetRecentPaths(RecentExcelOutputFiles, loaded.RecentExcelOutputFiles);
            AddRecentPath(RecentInputFiles, FilePath1, false);
            AddRecentPath(RecentInputFiles, FilePath2, false);
            AddRecentPath(RecentInputFiles, FilePath3, false);
            AddRecentPath(RecentDbcOutputFiles, OutputDbcFilePath, false);
            AddRecentPath(RecentExcelOutputFiles, OutputExcelFilePath, false);
        }
        catch
        {
            // Ignore settings load failures.
        }
        finally
        {
            _suppressSettingsSave = false;
        }
    }

    private void SaveSettings()
    {
        if (_suppressSettingsSave)
        {
            return;
        }

        _settings.FilePath1 = FilePath1;
        _settings.FilePath2 = FilePath2;
        _settings.FilePath3 = FilePath3;
        _settings.OutputDbcFilePath = OutputDbcFilePath;
        _settings.OutputExcelFilePath = OutputExcelFilePath;
        _settings.IncludeSignalGroupExcelColumns = IncludeSignalGroupExcelColumns;
        _settings.RecentInputFiles = RecentInputFiles.ToArray();
        _settings.RecentDbcOutputFiles = RecentDbcOutputFiles.ToArray();
        _settings.RecentExcelOutputFiles = RecentExcelOutputFiles.ToArray();

        var settingsPath = GetSettingsPath();
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
        }
        catch
        {
            // Ignore settings save failures.
        }
    }

    private static string GetSettingsPath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
    }

    private string GetInitialOpenDirectory(string target)
    {
        var candidate = target?.ToLowerInvariant() switch
        {
            "filepath1" => FilePath1,
            "filepath2" => FilePath2,
            "filepath3" => FilePath3,
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(candidate))
        {
            if (File.Exists(candidate))
            {
                return Path.GetDirectoryName(candidate);
            }

            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        if (!string.IsNullOrWhiteSpace(_settings.LastOpenDirectory) && Directory.Exists(_settings.LastOpenDirectory))
        {
            return _settings.LastOpenDirectory;
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    private void AddRecentPath(ObservableCollection<string> collection, string path, bool saveSettings = true)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var existing = collection.FirstOrDefault(item => string.Equals(item, path, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            collection.Remove(existing);
        }

        collection.Insert(0, path);
        while (collection.Count > MaxRecentPaths)
        {
            collection.RemoveAt(collection.Count - 1);
        }

        if (saveSettings)
        {
            SaveSettings();
        }
    }

    private static void SetRecentPaths(ObservableCollection<string> collection, IEnumerable<string> paths)
    {
        collection.Clear();
        if (paths == null)
        {
            return;
        }

        foreach (var path in paths.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase).Take(MaxRecentPaths))
        {
            collection.Add(path);
        }
    }

    private string GetInitialSaveDirectory(string target)
    {
        var candidate = target?.ToLowerInvariant() switch
        {
            "outputdbcfilepath" => OutputDbcFilePath,
            "outputexcelfilepath" => OutputExcelFilePath,
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(candidate))
        {
            if (File.Exists(candidate))
            {
                return Path.GetDirectoryName(candidate);
            }

            var directory = Path.GetDirectoryName(candidate);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                return directory;
            }
        }

        if (!string.IsNullOrWhiteSpace(_settings.LastSaveDirectory) && Directory.Exists(_settings.LastSaveDirectory))
        {
            return _settings.LastSaveDirectory;
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    private sealed class AppSettings
    {
        public string FilePath1 { get; set; }
        public string FilePath2 { get; set; }
        public string FilePath3 { get; set; }
        public string OutputDbcFilePath { get; set; }
        public string OutputExcelFilePath { get; set; }
        public string LastOpenDirectory { get; set; }
        public string LastSaveDirectory { get; set; }
        public bool IncludeSignalGroupExcelColumns { get; set; } = true;
        public string[] RecentInputFiles { get; set; }
        public string[] RecentDbcOutputFiles { get; set; }
        public string[] RecentExcelOutputFiles { get; set; }
    }
}
