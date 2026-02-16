# ============================================================================
# VPC LINKS - Conectam API Gateway aos Target Groups do NLB
# ============================================================================
#
# Cada VPC Link conecta o API Gateway a um Target Group especifico no NLB.
# Como NLBs nao entendem HTTP paths, criamos multiplos VPC Links, um para
# cada microsservico, e o roteamento por path acontece no API Gateway.
#
# ============================================================================

# Data source para obter informacoes da infraestrutura
data "terraform_remote_state" "infra" {
  backend = "s3"
  config = {
    bucket = var.infra_terraform_state_bucket
    key    = "infra/terraform.tfstate"
    region = var.aws_region
  }
}

# Security Group para VPC Links
resource "aws_security_group" "vpc_link_sg" {
  name        = "${var.project_identifier}-vpc-link-sg"
  description = "Security group para VPC Links do API Gateway"
  vpc_id      = data.terraform_remote_state.infra.outputs.vpc_principal_id

  egress {
    description = "Permitir todo trafego de saida"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_identifier}-vpc-link-sg"
  }
}

# ============================================================================
# VPC LINK - Cadastro Service
# ============================================================================

resource "aws_apigatewayv2_vpc_link" "cadastro" {
  name               = "${var.project_identifier}-vpc-link-cadastro"
  security_group_ids = [aws_security_group.vpc_link_sg.id]
  subnet_ids         = data.terraform_remote_state.infra.outputs.subnet_publica_ids

  tags = {
    Name    = "${var.project_identifier}-vpc-link-cadastro"
    Service = "cadastro"
  }
}

# Integracao API Gateway -> Cadastro Listener
resource "aws_apigatewayv2_integration" "cadastro" {
  api_id                 = aws_apigatewayv2_api.main.id
  integration_type       = "HTTP_PROXY"
  integration_uri        = data.terraform_remote_state.infra.outputs.cadastro_listener_arn
  integration_method     = "ANY"
  connection_type        = "VPC_LINK"
  connection_id          = aws_apigatewayv2_vpc_link.cadastro.id
  payload_format_version = "1.0"
}

# ============================================================================
# VPC LINK - Estoque Service
# ============================================================================

resource "aws_apigatewayv2_vpc_link" "estoque" {
  name               = "${var.project_identifier}-vpc-link-estoque"
  security_group_ids = [aws_security_group.vpc_link_sg.id]
  subnet_ids         = data.terraform_remote_state.infra.outputs.subnet_publica_ids

  tags = {
    Name    = "${var.project_identifier}-vpc-link-estoque"
    Service = "estoque"
  }
}

# Integracao API Gateway -> Estoque Listener
resource "aws_apigatewayv2_integration" "estoque" {
  api_id                 = aws_apigatewayv2_api.main.id
  integration_type       = "HTTP_PROXY"
  integration_uri        = data.terraform_remote_state.infra.outputs.estoque_listener_arn
  integration_method     = "ANY"
  connection_type        = "VPC_LINK"
  connection_id          = aws_apigatewayv2_vpc_link.estoque.id
  payload_format_version = "1.0"
}

# ============================================================================
# VPC LINK - OrdemServico Service
# ============================================================================

resource "aws_apigatewayv2_vpc_link" "ordemservico" {
  name               = "${var.project_identifier}-vpc-link-ordemservico"
  security_group_ids = [aws_security_group.vpc_link_sg.id]
  subnet_ids         = data.terraform_remote_state.infra.outputs.subnet_publica_ids

  tags = {
    Name    = "${var.project_identifier}-vpc-link-ordemservico"
    Service = "ordemservico"
  }
}

# Integracao API Gateway -> OrdemServico Listener
resource "aws_apigatewayv2_integration" "ordemservico" {
  api_id                 = aws_apigatewayv2_api.main.id
  integration_type       = "HTTP_PROXY"
  integration_uri        = data.terraform_remote_state.infra.outputs.ordemservico_listener_arn
  integration_method     = "ANY"
  connection_type        = "VPC_LINK"
  connection_id          = aws_apigatewayv2_vpc_link.ordemservico.id
  payload_format_version = "1.0"
}
