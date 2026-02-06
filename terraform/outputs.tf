# Outputs das funções Lambda
output "login_lambda_arn" {
  description = "ARN da função Lambda de Login"
  value       = aws_lambda_function.login.arn
}

output "login_lambda_name" {
  description = "Nome da função Lambda de Login"
  value       = aws_lambda_function.login.function_name
}

output "authorizer_lambda_arn" {
  description = "ARN da função Lambda Authorizer"
  value       = aws_lambda_function.authorizer.arn
}

output "authorizer_lambda_name" {
  description = "Nome da função Lambda Authorizer"
  value       = aws_lambda_function.authorizer.function_name
}

output "lambda_execution_role_arn" {
  description = "ARN da role de execução da Lambda"
  value       = aws_iam_role.lambda_execution_role.arn
}

# Outputs do API Gateway
output "api_gateway_id" {
  description = "ID do API Gateway"
  value       = aws_apigatewayv2_api.main.id
}

output "api_gateway_endpoint" {
  description = "Endpoint do API Gateway"
  value       = aws_apigatewayv2_api.main.api_endpoint
}

output "api_gateway_execution_arn" {
  description = "ARN de execução do API Gateway"
  value       = aws_apigatewayv2_api.main.execution_arn
}

output "vpc_link_id" {
  description = "ID do VPC Link"
  value       = aws_apigatewayv2_vpc_link.eks.id
}

# Outputs do CloudWatch Logs
output "api_gateway_log_group_name" {
  description = "Nome do CloudWatch Log Group do API Gateway"
  value       = aws_cloudwatch_log_group.api_gateway.name
}

output "api_gateway_log_group_arn" {
  description = "ARN do CloudWatch Log Group do API Gateway"
  value       = aws_cloudwatch_log_group.api_gateway.arn
}
