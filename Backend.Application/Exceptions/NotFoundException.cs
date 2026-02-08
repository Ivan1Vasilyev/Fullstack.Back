namespace Backend.Application.Exceptions
{
    public class NotFoundException(string message) : ApplicationCustomException(message, "Не найдено");
}
