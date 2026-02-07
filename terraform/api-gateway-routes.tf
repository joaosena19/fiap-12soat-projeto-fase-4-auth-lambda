# ============================================================================
# ROTAS PÚBLICAS - Sem autenticação
# ============================================================================

# Rota de autenticação - Lambda de login
resource "aws_apigatewayv2_route" "auth_post" {
  api_id    = aws_apigatewayv2_api.main.id
  route_key = "POST /auth/authenticate"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_login.id}"
  
  depends_on = [aws_apigatewayv2_integration.lambda_login]
}

# Rota de webhook - Lambda de login
resource "aws_apigatewayv2_route" "webhook_post" {
  api_id    = aws_apigatewayv2_api.main.id
  route_key = "POST /webhook"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_login.id}"
  
  depends_on = [aws_apigatewayv2_integration.lambda_login]
}

# ============================================================================
# ROTAS PROTEGIDAS - CADASTRO SERVICE
# ============================================================================

# Rotas de Clientes
resource "aws_apigatewayv2_route" "clientes" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/clientes/{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.cadastro.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.cadastro,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

resource "aws_apigatewayv2_route" "clientes_root" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/clientes"
  target             = "integrations/${aws_apigatewayv2_integration.cadastro.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.cadastro,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

# Rotas de Veículos
resource "aws_apigatewayv2_route" "veiculos" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/veiculos/{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.cadastro.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.cadastro,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

resource "aws_apigatewayv2_route" "veiculos_root" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/veiculos"
  target             = "integrations/${aws_apigatewayv2_integration.cadastro.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.cadastro,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

# Rotas de Serviços
resource "aws_apigatewayv2_route" "servicos" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/servicos/{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.cadastro.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.cadastro,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

resource "aws_apigatewayv2_route" "servicos_root" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/servicos"
  target             = "integrations/${aws_apigatewayv2_integration.cadastro.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.cadastro,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

# Rotas de Usuários
resource "aws_apigatewayv2_route" "usuarios" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/usuarios/{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.cadastro.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.cadastro,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

resource "aws_apigatewayv2_route" "usuarios_root" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/usuarios"
  target             = "integrations/${aws_apigatewayv2_integration.cadastro.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.cadastro,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

# ============================================================================
# ROTAS PROTEGIDAS - ESTOQUE SERVICE
# ============================================================================

# Rotas de Estoque
resource "aws_apigatewayv2_route" "estoque" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/estoque/{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.estoque.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.estoque,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

resource "aws_apigatewayv2_route" "estoque_root" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/estoque"
  target             = "integrations/${aws_apigatewayv2_integration.estoque.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.estoque,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

# ============================================================================
# ROTAS PROTEGIDAS - ORDEM DE SERVIÇO SERVICE
# ============================================================================

# Rotas de Ordens de Serviço
resource "aws_apigatewayv2_route" "ordens_servico" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/ordens-servico/{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.ordemservico.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.ordemservico,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}

resource "aws_apigatewayv2_route" "ordens_servico_root" {
  api_id             = aws_apigatewayv2_api.main.id
  route_key          = "ANY /api/ordens-servico"
  target             = "integrations/${aws_apigatewayv2_integration.ordemservico.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_auth.id
  
  depends_on = [
    aws_apigatewayv2_integration.ordemservico,
    aws_apigatewayv2_authorizer.lambda_auth
  ]
}