namespace Backend.Exceptions
{
    public class ConflictException : ApplicationCustomException
    {
        public ConflictException(string tableName, Dictionary<string, object?> fieldValues)
           : base(FormatMessage(tableName, fieldValues), "Конфликт с существующими данными") { }

        public ConflictException(string tableName, string fieldName, object? fieldValue)
            : base($"Поле '{fieldName}' со значением '{fieldValue ?? "null"}' уже существует в коллекции {tableName}", "Конфликт с существующими данными") { }


        private static string FormatMessage(string tableName, Dictionary<string, object?> fieldValues)
        {
            if (fieldValues == null || fieldValues.Count == 0)
                return $"Запись уже существует в коллекции {tableName}";

            var fields = string.Join(", ", fieldValues.Select(kv => $"'{kv.Key}' = '{kv.Value ?? "null"}'"));

            if (fieldValues.Count == 1)
                return $"Поле {fields} уже существует в коллекции {tableName}";
            else
                return $"Комбинация полей {fields} уже существует в коллекции {tableName}";
        }
    }
}
