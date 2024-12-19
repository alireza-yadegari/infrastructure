[Serializable]
public class ExternalLoginException : Exception
{
    public ExternalLoginException(string message)
        : base(message)
    {
        Value = message;
    }

    public int StatusCode { get; } = 400;

    public object? Value { get; }
}