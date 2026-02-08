namespace Backend.Application.Exceptions
{
    public class InfrastructureException(string message) : ApplicationCustomException(message, "Ошибка базы данных");
}
