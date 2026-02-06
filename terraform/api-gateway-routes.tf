# Rotas públicas (sem autenticação) - para Lambda de login
resource "aws_apigatewayv2_route" "auth_post" {
  api_id    = aws_apigatewayv2_api.main.id
  route_key = "POST /auth/authenticate"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_login.id}"
  
  depends_on = [aws_apigatewayv2_integration.lambda_login]
}

resource "aws_apigatewayv2_route" "webhook_post" {
  api_id    = aws_apigatewayv2_api.main.id
  route_key = "POST /webhook"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_login.id}"
  
  depends_on = [aws_apigatewayv2_integration.lambda_login]
}

# Rota protegida (com Lambda Authorizer) - proxy para EKS
resource "aws_apigatewayv2_route" "protected_proxy" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.eks.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.eks,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

# Rota para health check (pública)
resource "aws_apigatewayv2_route" "health_get" {
  api_id    = aws_apigatewayv2_api.main.id
  route_key = "GET /health"
  target    = "integrations/${aws_apigatewayv2_integration.eks.id}"
  
  depends_on = [aws_apigatewayv2_integration.eks]
}

# Rota catch-all protegida para outras rotas da aplicação
resource "aws_apigatewayv2_route" "protected_catchall" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.eks.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.eks,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}