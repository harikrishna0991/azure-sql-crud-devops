output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "app_service_name" {
  value = azurerm_linux_web_app.app.name
}

output "app_service_hostname" {
  value = azurerm_linux_web_app.app.default_hostname
}

output "app_service_managed_identity_principal_id" {
  value = azurerm_linux_web_app.app.identity[0].principal_id
}

output "key_vault_name" {
  value = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  value = azurerm_key_vault.main.vault_uri
}

output "sql_server_name" {
  value = azurerm_mssql_server.main.name
}

output "sql_fully_qualified_domain_name" {
  value = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "sql_database_name" {
  value = azurerm_mssql_database.main.name
}

output "application_insights_name" {
  value = azurerm_application_insights.app.name
}
