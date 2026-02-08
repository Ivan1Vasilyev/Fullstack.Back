namespace Backend.Domain.Common.Exceptions
{
    public class ForeignKeyException(string tableName) : DomainException($"Нельзя выполнить операцию из-за связанных данных в таблице {tableName}");
}
