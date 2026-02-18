[![Deploy](https://github.com/joaosena19/fiap-12soat-projeto-fase-4-auth-lambda/actions/workflows/deploy.yaml/badge.svg)](https://github.com/joaosena19/fiap-12soat-projeto-fase-4-auth-lambda/actions/workflows/deploy.yaml)

# Identificação

Aluno: João Pedro Sena Dainese  
Registro FIAP: RM365182  

Turma 12SOAT - Software Architecture  
Grupo individual  
Grupo 13  

Discord: joaodainese  
Email: joaosenadainese@gmail.com  

## Sobre este Repositório

Este repositório contém apenas parte do projeto completo da Fase 4. Para visualizar a documentação completa, diagramas de arquitetura, e todos os componentes do projeto, acesse: [Documentação Completa - Fase 4](https://github.com/joaosena19/fiap-12soat-projeto-fase-4-documentacao)

## Descrição

Funções AWS Lambda para autenticação: login (geração de JWT) e authorizer (validação de tokens). Integrado ao API Gateway e conectado ao banco de dados do microsserviço de Cadastro para validar credenciais.

## Tecnologias Utilizadas

- **.NET 8** - Runtime
- **AWS Lambda** - Serverless
- **AWS API Gateway** - Roteamento e autorização
- **JWT** - Geração e validação de tokens
- **PostgreSQL** - Leitura de credenciais (banco do Cadastro)
- **Terraform** - Provisionamento da Lambda, API Gateway e VPC Links
- **SonarCloud** - Análise estática de qualidade

## Autenticação

Para entender o fluxo completo de autenticação e autorização, consulte: [Autenticação](https://github.com/joaosena19/fiap-12soat-projeto-fase-4-documentacao/blob/main/4.%20Autenticação/1_autenticacao.md)