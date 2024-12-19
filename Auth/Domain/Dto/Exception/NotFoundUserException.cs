[Serializable]
public class NotFoundUserException : Exception
{
    public NotFoundUserException(string message)
        : base(message)
    {
        Value = message;
    }

    public int StatusCode { get; } = 400;

    public object? Value { get; }
}