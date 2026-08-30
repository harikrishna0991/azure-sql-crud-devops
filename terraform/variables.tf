variable "location" {
  description = "Azure region"
  type        = string
  default     = "Central India"
}

variable "resource_group_name" {
  description = "Application resource group"
  type        = string
  default     = "rg-azure-crud-dev"
}

variable "project_name" {
  description = "Project name"
  type        = string
  default     = "azurecrud"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "vnet_address_space" {
  description = "VNet address space"
  type        = list(string)
  default     = ["10.10.0.0/16"]
}

variable "app_service_subnet_prefix" {
  description = "App Service integration subnet"
  type        = list(string)
  default     = ["10.10.1.0/24"]
}

variable "private_endpoint_subnet_prefix" {
  description = "Private Endpoint subnet"
  type        = list(string)
  default     = ["10.10.2.0/24"]
}

variable "app_service_plan_sku" {
  description = "App Service Plan SKU"
  type        = string
  default     = "B1"
}

variable "sql_database_name" {
  description = "Azure SQL database name"
  type        = string
  default     = "TodoDb"
}

variable "sql_database_sku" {
  description = "Azure SQL Database SKU"
  type        = string
  default     = "Basic"
}

variable "sql_server_name" {
  description = "Globally unique Azure SQL server name"
  type        = string
  default     = "azurecrud-dev-sql-2026"
}

variable "sql_admin_login" {
  description = "Microsoft Entra administrator login for Azure SQL"
  type        = string
}

variable "sql_admin_object_id" {
  description = "Microsoft Entra administrator object ID for Azure SQL"
  type        = string
}
variable "github_actions_uami_principal_id" {
  description = "Principal ID of the GitHub Actions user-assigned managed identity"
  type        = string
}
