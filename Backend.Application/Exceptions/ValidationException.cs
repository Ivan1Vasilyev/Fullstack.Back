namespace Backend.Application.Exceptions
{
    public class ValidationException : ApplicationCustomException
    {
        public ValidationException(List<string> messages)
            : base(string.Join('\n', messages), "Получены некорректные данные") { }

        public ValidationException(string message)
            : base(message, "Получены некорректные данные") { }
    }
}
