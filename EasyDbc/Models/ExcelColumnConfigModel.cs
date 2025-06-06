﻿namespace EasyDbc.Models
{
    public class ExcelColumnConfigModel
    {
        public string Header { get; set; }
        public bool IsVisible { get; set; }
        public int ColumnIndex { get; set; }
        public double ColumnWidth { get; set; } 
    }
    public enum DictionaryColumnKey
    {
        MessageName,
        FrameFormat,
        ID,
        MessageSendType,
        CycleTime,
        DataLength,
        SignalName,
        Description,
        ByteOrder,
        StartBit,
        BitLength,
        DataType,
        Factor,
        Offset,
        MinimumPhysical,
        MaximumPhysical,
        InitialValue,
        Unit,
        ValueTable,
    }
    public enum WriteStatus
    {
        Success,
        PathError,
        FormatError,
        WritePermissionError,
        UnknownError
    }
    public enum ExcelParserState
    {
        Success,
        PathError,
        FormatError,
        ReadPermissionError,
        ReadNullError,
        UnknownError
    }
    public enum DbcOrderBy
    {
        Name,
        Id,
        Transmitter,
    }
    public enum UpdateColumnConfigState
    {
        Success,
        ColumnIndexError,
        HeaderError,
        ColumnKeyNotExists,
        UnknownError
    }
    public enum DbcProtocolType
    {
        CAN,
        J1939,
        CANOpen,
        UDS,
        ISO15765,
        FlexRay,
        LIN,
        Ethernet,
    }

}
