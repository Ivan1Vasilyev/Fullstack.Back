namespace Backend.Domain.Common.Exceptions
{
    public class EntityNotFoundException(string entityType, object entityId) : DomainException($"{entityType} с id {entityId} не найден");
}
