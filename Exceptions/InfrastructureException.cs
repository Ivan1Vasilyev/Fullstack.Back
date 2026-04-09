namespace Backend.Exceptions
{
    public class InfrastructureException(string message) : ApplicationCustomException(message, "Ошибка базы данных");
}
