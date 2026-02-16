# Data source para obter informacoes do banco de Cadastros (que contem usuarios, roles, clientes)
data "terraform_remote_state" "cadastro_db" {
  backend = "s3"
  config = {
    bucket = var.banco_terraform_state_bucket
    key    = "fase4/cadastro-database/terraform.tfstate"
    region = var.aws_region
  }
}

# Security Group para Lambda acessar RDS
resource "aws_security_group" "lambda_sg" {
  name        = "${var.project_identifier}-lambda-sg"
  description = "Security group para Lambda acessar RDS"
  vpc_id      = data.terraform_remote_state.infra.outputs.vpc_principal_id

  egress {
    description = "Acesso PostgreSQL"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = [data.terraform_remote_state.infra.outputs.vpc_principal_cidr]
  }

  egress {
    description = "HTTPS para internet"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    description = "HTTP para internet"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_identifier}-lambda-sg"
  }
}

# Fonte de dados para criar arquivo zip do codigo da Lambda
data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/../src/AuthLambda/bin/Release/net8.0/publish"
  output_path = "${path.module}/lambda_function.zip"
}

# Lambda Function para Login (geracao de tokens)
resource "aws_lambda_function" "login" {
  filename         = data.archive_file.lambda_zip.output_path
  function_name    = "${var.project_identifier}-login-lambda"
  role             = aws_iam_role.lambda_execution_role.arn
  handler          = "AuthLambda::AuthLambda.LoginHandler::FunctionHandler"
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256
  runtime          = var.lambda_runtime
  timeout          = var.lambda_timeout
  memory_size      = var.lambda_memory_size

  # New Relic Layer
  layers = [
    "arn:aws:lambda:us-east-1:451483290750:layer:NewRelicLambdaExtension:37"
  ]

  # Configuracao VPC para acessar RDS
  vpc_config {
    subnet_ids         = data.terraform_remote_state.infra.outputs.subnet_publica_ids
    security_group_ids = [aws_security_group.lambda_sg.id]
  }

  environment {
    variables = {
      Jwt__Key                         = var.jwt_key
      Jwt__Issuer                      = var.jwt_issuer
      Jwt__Audience                    = var.jwt_audience
      DatabaseConnection__Host         = data.terraform_remote_state.cadastro_db.outputs.db_instance_address
      DatabaseConnection__Port         = tostring(data.terraform_remote_state.cadastro_db.outputs.db_instance_port)
      DatabaseConnection__DatabaseName = data.terraform_remote_state.cadastro_db.outputs.db_name
      DatabaseConnection__User         = data.terraform_remote_state.cadastro_db.outputs.db_username
      DatabaseConnection__Password     = var.db_password

      # New Relic Configuration
      NEW_RELIC_ACCOUNT_ID                   = var.new_relic_account_id
      NEW_RELIC_LICENSE_KEY                  = var.new_relic_license_key
      NEW_RELIC_EXTENSION_SEND_FUNCTION_LOGS = "true"
      NEW_RELIC_APP_NAME                     = "${var.project_identifier}-login-lambda"
    }
  }

  tags = {
    Name = "${var.project_identifier}-login-lambda"
  }

  depends_on = [aws_iam_role_policy_attachment.lambda_vpc_access]
}

# Lambda Function para Authorizer (validacao de tokens)
resource "aws_lambda_function" "authorizer" {
  filename         = data.archive_file.lambda_zip.output_path
  function_name    = "${var.project_identifier}-authorizer-lambda"
  role             = aws_iam_role.lambda_execution_role.arn
  handler          = "AuthLambda::AuthLambda.AuthorizerHandler::FunctionHandler"
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256
  runtime          = var.lambda_runtime
  timeout          = var.lambda_timeout
  memory_size      = var.lambda_memory_size

  # New Relic Layer
  layers = [
    "arn:aws:lambda:us-east-1:451483290750:layer:NewRelicLambdaExtension:37"
  ]

  environment {
    variables = {
      Jwt__Key      = var.jwt_key
      Jwt__Issuer   = var.jwt_issuer
      Jwt__Audience = var.jwt_audience

      # New Relic Configuration
      NEW_RELIC_ACCOUNT_ID                   = var.new_relic_account_id
      NEW_RELIC_LICENSE_KEY                  = var.new_relic_license_key
      NEW_RELIC_EXTENSION_SEND_FUNCTION_LOGS = "true"
      NEW_RELIC_APP_NAME                     = "${var.project_identifier}-authorizer-lambda"
    }
  }

  tags = {
    Name = "${var.project_identifier}-authorizer-lambda"
  }
}
