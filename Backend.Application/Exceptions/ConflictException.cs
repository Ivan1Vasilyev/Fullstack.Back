namespace Backend.Application.Exceptions
{
    public class ConflictException(string message) : ApplicationCustomException(message, "Конфликт с существующими данными");
}
