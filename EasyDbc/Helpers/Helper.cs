﻿using System.Text;
using System.Text.RegularExpressions;

namespace EasyDbc.Helpers
{
    public static class Helper
    {
        public const string Space = " ";
        public const string Comma = ",";
        public const string DoubleQuotes = "\"";

        private static readonly string[] SpaceArray = new[] { Space };

        public static string[] SplitBySpace(this string value)
        {
            return value.Split(SpaceArray, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ConvertToMultiLine(string[] records, int offset)
        {
            var sb = new StringBuilder();
            for (var i = offset; i < records.Length - 1; i += 2)
            {
                sb.AppendFormat("{0} {1} {2}", records[i], records[i+1], '\n');
            }

            return sb.ToString();
        }

        public static string ConcatenateTextComment(string[] strings, int startingIndex)
        {
            var sb = new StringBuilder();
            foreach(var s in strings )
            {
                sb.AppendLine(Regex.Replace(s, @"""|;", ""));
            }
            var commentText = sb.ToString().Substring(startingIndex);
            return commentText.Substring(0, commentText.Length - 2);
        }
    }

}