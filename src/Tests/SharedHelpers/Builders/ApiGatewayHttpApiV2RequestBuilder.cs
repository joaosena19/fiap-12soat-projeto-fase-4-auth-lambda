using Amazon.Lambda.APIGatewayEvents;

namespace Tests.SharedHelpers.Builders;

public class ApiGatewayHttpApiV2RequestBuilder
{
    private Dictionary<string, string> _headers;

    public ApiGatewayHttpApiV2RequestBuilder()
    {
        _headers = new Dictionary<string, string>();
    }

    public ApiGatewayHttpApiV2RequestBuilder ComHeaders(Dictionary<string, string> headers)
    {
        _headers = headers;
        return this;
    }

    public ApiGatewayHttpApiV2RequestBuilder ComAuthorizationHeader(string authorizationValue)
    {
        _headers["authorization"] = authorizationValue;
        return this;
    }

    public APIGatewayHttpApiV2ProxyRequest Build()
    {
        return new APIGatewayHttpApiV2ProxyRequest
        {
            Headers = _headers,
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
            {
                RequestId = Guid.NewGuid().ToString(),
                Stage = "test",
                RouteKey = "POST /auth/authorize",
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                {
                    Method = "POST",
                    Path = "/auth/authorize"
                }
            },
            RouteKey = "POST /auth/authorize",
            RawPath = "/auth/authorize"
        };
    }
}
