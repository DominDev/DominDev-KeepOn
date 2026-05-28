namespace KeepOn;

internal sealed class PowerRequestException : Exception
{
    public PowerRequestException(string operation, int errorCode)
        : base($"{operation} failed. Win32 error: {errorCode}.")
    {
        Operation = operation;
        ErrorCode = errorCode;
    }

    public string Operation { get; }

    public int ErrorCode { get; }
}
