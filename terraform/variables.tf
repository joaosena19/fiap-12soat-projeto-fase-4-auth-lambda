variable "aws_region" {
  description = "AWS region onde os recursos serao criados"
  type        = string
  default     = "us-east-1"
}

variable "project_identifier" {
  description = "Identificador do projeto para nomeacao de recursos"
  type        = string
  default     = "fiap-12soat-fase4"
}

variable "lambda_runtime" {
  description = "Runtime da funcao Lambda"
  type        = string
  default     = "dotnet8"
}

variable "lambda_timeout" {
  description = "Timeout da funcao Lambda em segundos"
  type        = number
  default     = 30
}

variable "lambda_memory_size" {
  description = "Memoria alocada para a funcao Lambda em MB"
  type        = number
  default     = 512
}

variable "jwt_key" {
  description = "Chave secreta para geracao de tokens JWT"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "Issuer do token JWT"
  type        = string
  default     = "OficinaMecanicaApi"
}

variable "jwt_audience" {
  description = "Audience do token JWT"
  type        = string
  default     = "AuthorizedServices"
}

# Variaveis para remote state da infraestrutura
variable "infra_terraform_state_bucket" {
  description = "Nome do bucket S3 onde esta o state da infraestrutura"
  type        = string
  default     = "fiap-12soat-fase4-joao-dainese"
}

# Variaveis para remote state do banco
variable "banco_terraform_state_bucket" {
  description = "Nome do bucket S3 onde esta o state do banco"
  type        = string
  default     = "fiap-12soat-fase4-joao-dainese"
}

# Senha do banco (passada via workflow)
variable "db_password" {
  description = "Senha do banco de dados"
  type        = string
  sensitive   = true
}

variable "new_relic_account_id" {
  description = "ID da conta do New Relic"
  type        = string
  sensitive   = true
}

variable "new_relic_license_key" {
  description = "Chave de licenca do New Relic (Ingest License)"
  type        = string
  sensitive   = true
}