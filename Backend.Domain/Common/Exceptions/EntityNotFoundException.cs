namespace Backend.Domain.Common.Exceptions
{
    public class EntityNotFoundException(string entityType, string fieldName, object fieldValue) : DomainException($"{entityType} с {fieldName} = {fieldValue} не найден");
}
