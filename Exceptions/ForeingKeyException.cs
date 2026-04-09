namespace Backend.Exceptions
{
    public class ForeignKeyException(string tableName) 
        : ApplicationCustomException($"Нельзя выполнить операцию из-за связанных данных в таблице {tableName}", "Ошибка базы данных");
}
