using System.Collections.Concurrent;
using System.Data;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders.Tariffs.ColumnMappings
{
	public class ColumnMappingData
	{
		string[] _columns;
		bool _isRegex;
		Regex[] _regexes;

		ConcurrentDictionary<DataTable, int> _columnIndexes = new ConcurrentDictionary<DataTable, int>();
		ConcurrentDictionary<DataTable, int[]> _columnsIndexes = new ConcurrentDictionary<DataTable, int[]>();

		public ColumnMappingData(bool isRegex, params string[] columnPatterns)
		{
			_columns = columnPatterns.Select(x => x.Trim()).ToArray();
			_isRegex = isRegex;

			if (isRegex)
			{
				_regexes = columnPatterns.Select(x => new Regex(x, RegexOptions.Compiled)).ToArray();
			}
		}

		public ColumnMappingData(params string[] columnPatterns) : this(true, columnPatterns.Select(NormalizeRegex).ToArray())
		{
		}

		private static string NormalizeRegex(string str)
		{
			str = str.Trim();
			str = str.Replace("\\", "\\\\")
					 .Replace(".", "\\.")
					 .Replace("*", "\\*")
					 .Replace("+", "\\+")
					 .Replace("(", "\\(").Replace(")", "\\)")
					 .Replace("[", "\\[").Replace("]", "\\]");

			str = Regex.Replace(str, "\\s+", "\\s*");
			return "^\\s*" + str + "\\s*$";
		}

		private int GetColumnIndex(DataRow row)
		{
			if (_columnIndexes.TryGetValue(row.Table, out var cachedColumnIndex))
			{
				return cachedColumnIndex;
			}

			for (var i = 0; i < row.Table.Columns.Count; i++)
			{
				var columnName = row.Table.Columns[i].ColumnName.Trim();
				if (_isRegex)
				{
					if (_regexes.Any(x => x.IsMatch(columnName)))
					{
						_columnIndexes[row.Table] = i;
						return i;
					}
				}
				else
				{
					if (_columns.Any(x => x.Equals(columnName, StringComparison.InvariantCultureIgnoreCase)))
					{
						_columnIndexes[row.Table] = i;
						return i;
					}
				}
			}

			_columnIndexes[row.Table] = -1;

			return -1;
		}

		public string? GetValue(DataRow row)
		{
			var colIndex = GetColumnIndex(row);
			if (colIndex == -1) return null;
			return row[colIndex] as string;
		}

		private int[] GetColumnIndexes(DataRow row)
		{
			if (_columnsIndexes.TryGetValue(row.Table, out var indexes))
			{
				return indexes;
			}

			var cache = new HashSet<int>();

			for (var i = 0; i < row.Table.Columns.Count; i++)
			{
				var columnName = row.Table.Columns[i].ColumnName.Trim();
				if (_isRegex)
				{
					if (_regexes.Any(x => x.IsMatch(columnName)))
					{
						cache.Add(i);
					}
				}
				else
				{
					if (_columns.Any(x => x.Equals(columnName, StringComparison.InvariantCultureIgnoreCase)))
					{
						cache.Add(i);
					}
				}
			}

			var result = cache.ToArray();
			_columnsIndexes[row.Table] = result;

			return result;
		}

		public Dictionary<string, string> GetValues(DataRow row)
		{
			var columns = GetColumnIndexes(row);

			var result = new Dictionary<string, string>();

			foreach (var column in columns)
			{
				result[row.Table.Columns[column].ColumnName] = row[column] as string;
			}

			return result;
		}
	}
}
