[Serializable]
public class RegisterException : Exception
{
    public RegisterException(string message)
        : base(message)
    {
        Value = message;
    }

    public int StatusCode { get; } = 400;

    public object? Value { get; }
}