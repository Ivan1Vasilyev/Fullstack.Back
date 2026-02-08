namespace Backend.Application.Exceptions
{
    public abstract class ApplicationCustomException(string message, string title) : Exception(message)
    {
        public string Title { get; } = title;
    }
}
