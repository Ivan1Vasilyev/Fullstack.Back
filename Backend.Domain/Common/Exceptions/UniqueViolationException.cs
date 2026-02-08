namespace Backend.Domain.Common.Exceptions
{
    public class UniqueViolationException(string FieldName, string FieldValue, string TableName) 
        : DomainException($"Поле '{FieldName}' со значением '{FieldValue}' уже существует в коллекции {TableName}");
}
