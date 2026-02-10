using Amazon.Lambda.Core;

namespace Tests.SharedHelpers.Lambda;

public class LambdaContextBuilder
{
    private ILambdaLogger _logger;

    public LambdaContextBuilder()
    {
        _logger = new LambdaLoggerSpy();
    }

    public static ILambdaContext Criar()
    {
        return new LambdaContextBuilder().Build();
    }

    public LambdaContextBuilder ComLogger(ILambdaLogger logger)
    {
        _logger = logger;
        return this;
    }

    public ILambdaContext Build()
    {
        return new FakeLambdaContext(_logger);
    }

    private class FakeLambdaContext : ILambdaContext
    {
        public FakeLambdaContext(ILambdaLogger logger)
        {
            Logger = logger;
            AwsRequestId = Guid.NewGuid().ToString();
            FunctionName = "TestFunction";
            FunctionVersion = "1.0";
            LogGroupName = "/aws/lambda/test";
            LogStreamName = "test-stream";
            MemoryLimitInMB = 256;
            RemainingTime = TimeSpan.FromMinutes(15);
        }

        public string AwsRequestId { get; }
        public IClientContext ClientContext => null!;
        public string FunctionName { get; }
        public string FunctionVersion { get; }
        public ICognitoIdentity Identity => null!;
        public string InvokedFunctionArn => "arn:aws:lambda:us-east-1:123456789012:function:TestFunction";
        public ILambdaLogger Logger { get; }
        public string LogGroupName { get; }
        public string LogStreamName { get; }
        public int MemoryLimitInMB { get; }
        public TimeSpan RemainingTime { get; }
    }
}
