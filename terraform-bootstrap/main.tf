resource "azurerm_resource_group" "terraform_state" {
  name     = var.resource_group_name
  location = var.location

  tags = {
    Purpose   = "Terraform State"
    ManagedBy = "Terraform"
  }
}

resource "azurerm_storage_account" "terraform_state" {
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.terraform_state.name
  location                 = azurerm_resource_group.terraform_state.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  min_tls_version               = "TLS1_2"
  public_network_access_enabled = true

  shared_access_key_enabled = false
  local_user_enabled        = false

  default_to_oauth_authentication = true

  allow_nested_items_to_be_public = false

  blob_properties {
    versioning_enabled = true
  }

  tags = {
    Purpose   = "Terraform State"
    ManagedBy = "Terraform"
  }
}

resource "azurerm_storage_container" "terraform_state" {
  name                  = var.container_name
  storage_account_id    = azurerm_storage_account.terraform_state.id
  container_access_type = "private"
}
