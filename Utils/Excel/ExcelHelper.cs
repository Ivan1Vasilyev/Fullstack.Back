namespace Backend.Utils.Excel
{
	public class ExcelHelper : IExcelHelper
	{
		public string GetColumnNameByIndex(int index)
		{
			var chIndex = index % 26;
			var ch = (char)('A' + chIndex);
			var dim = "";
			var remain = (index - chIndex) / 26;

			if (remain > 0)
			{
				dim = GetColumnNameByIndex(remain - 1);
			}

			return dim + ch;

		}
	}
}
