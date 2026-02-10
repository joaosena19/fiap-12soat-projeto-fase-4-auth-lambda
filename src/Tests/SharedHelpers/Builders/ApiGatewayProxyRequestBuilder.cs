using Amazon.Lambda.APIGatewayEvents;

namespace Tests.SharedHelpers.Builders;

public class ApiGatewayProxyRequestBuilder
{
    private string? _body;
    private Dictionary<string, string> _headers;

    public ApiGatewayProxyRequestBuilder()
    {
        _body = null;
        _headers = new Dictionary<string, string>();
    }

    public ApiGatewayProxyRequestBuilder ComBody(string body)
    {
        _body = body;
        return this;
    }

    public ApiGatewayProxyRequestBuilder ComHeaders(Dictionary<string, string> headers)
    {
        _headers = headers;
        return this;
    }

    public APIGatewayProxyRequest Build()
    {
        return new APIGatewayProxyRequest
        {
            Body = _body,
            Headers = _headers,
            HttpMethod = "POST",
            Path = "/auth/login",
            Resource = "/auth/login",
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                RequestId = Guid.NewGuid().ToString(),
                Stage = "test"
            }
        };
    }
}
