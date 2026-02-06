provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project   = "fiap-12soat-fase3"
      ManagedBy = "Terraform"
      Component = "Lambda-Auth"
    }
  }
}
