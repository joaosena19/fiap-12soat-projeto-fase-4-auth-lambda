using Amazon.Lambda.Core;

namespace Tests.SharedHelpers.Lambda;

public class LambdaLoggerSpy : ILambdaLogger
{
    private readonly List<string> _informationLogs = new();
    private readonly List<string> _warningLogs = new();
    private readonly List<string> _errorLogs = new();

    public IReadOnlyList<string> InformationLogs => _informationLogs.AsReadOnly();
    public IReadOnlyList<string> WarningLogs => _warningLogs.AsReadOnly();
    public IReadOnlyList<string> ErrorLogs => _errorLogs.AsReadOnly();

    public void Log(string message)
    {
        _informationLogs.Add(message);
    }

    public void LogLine(string message)
    {
        _informationLogs.Add(message);
    }

    public void LogInformation(string message)
    {
        _informationLogs.Add(message);
    }

    public void LogWarning(string message)
    {
        _warningLogs.Add(message);
    }

    public void LogError(string message)
    {
        _errorLogs.Add(message);
    }
}
