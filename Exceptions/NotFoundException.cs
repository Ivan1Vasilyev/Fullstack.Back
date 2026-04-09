namespace Backend.Exceptions
{
    public class NotFoundException(string entityType, string fieldName, object fieldValue) 
        : ApplicationCustomException($"{entityType} с {fieldName} = {fieldValue} не найден", "Не найдено");
}
