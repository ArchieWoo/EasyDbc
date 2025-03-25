using EasyDbc.Models;

namespace EasyDbc.Contracts
{
    public interface IExcelParser
    {
        ExcelParserState ParseFirstSheetFromPath(string path, out Dbc dbc, DbcOrderBy dbcOrderBy);
        ExcelParserState ParseSheetNameFromPath(string path, string sheetName, out Dbc dbc, DbcOrderBy dbcOrderBy);
        ExcelParserState ParseSheetIndexFromPath(string path, int sheetIndex, out Dbc dbc, DbcOrderBy dbcOrderBy);

        UpdateColumnConfigState UpdateColumnConfigWithIndex(DictionaryColumnKey columnKey, int columnIndex);
        UpdateColumnConfigState UpdateColumnConfigWithIndex(string columnKey, int columnIndex);
        UpdateColumnConfigState UpdateColumnConfigWithName(DictionaryColumnKey columnKey, string columnIndexName);
        UpdateColumnConfigState UpdateColumnConfigWithName(string columnKey, string columnIndexName);

        void SetNodeStartIndex(int nodeStartIndex);
        void SetNodeStartIndex(string excelColoumnName);
        int GetNodeStartIndex();
        string GetColumnIndexName(DictionaryColumnKey columnKey);
        int GetColumnIndex(DictionaryColumnKey columnKey);
        void SetProtocolType(DbcProtocolType protocolType);
        void SetMessageFieldRowStartOffset(int startIndex = 1);
        void SetNodeRowIndex(int nodeRowIndex = 0);
        bool CheckColumnIndexConfiction(out List<int> confictionIndexList);
    }
}
