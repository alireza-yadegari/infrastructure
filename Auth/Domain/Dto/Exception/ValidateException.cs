[Serializable]
public class ValidateException : Exception
{
    public ValidateException(string message)
        : base(message)
    {
        Value = message;
    }

    public int StatusCode { get; } = 400;

    public object? Value { get; }
}