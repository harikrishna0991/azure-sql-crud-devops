terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state-azurecrud"
    storage_account_name = "tfstateazurecrud2026new"
    container_name       = "tfstate"
    key                  = "dev/terraform.tfstate"

    use_azuread_auth = true
    use_cli          = true
  }
}