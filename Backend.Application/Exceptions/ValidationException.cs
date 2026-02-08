namespace Backend.Application.Exceptions
{
    public class ValidationException(string message) : ApplicationCustomException(message, "Получены некорректные данные");
}
