[Serializable]
public class LoginException : Exception
{
    public LoginException(string message)
        : base(message)
    {
        Value = message;
    }

    public int StatusCode { get; } = 401;

    public object? Value { get; }
}