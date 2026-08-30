output "state_resource_group_name" {
  value = azurerm_resource_group.terraform_state.name
}

output "state_storage_account_name" {
  value = azurerm_storage_account.terraform_state.name
}

output "state_container_name" {
  value = azurerm_storage_container.terraform_state.name
}
