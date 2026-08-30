resource "azurerm_service_plan" "app" {
  name                = "${local.name_prefix}-plan"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  os_type  = "Linux"
  sku_name = var.app_service_plan_sku

  tags = local.common_tags
}

resource "azurerm_linux_web_app" "app" {
  name                = "${local.name_prefix}-app"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.app.id

  enabled                       = true
  https_only                    = true
  public_network_access_enabled = true

  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false

  client_certificate_enabled = false

  virtual_network_subnet_id = azurerm_subnet.app_service.id

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on = true

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5

    minimum_tls_version = "1.2"
    ftps_state          = "Disabled"

    vnet_route_all_enabled = true

    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.app.connection_string

    AZURE_SQL_CONNECTIONSTRING = "@Microsoft.KeyVault(SecretUri=https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/database-connection)"
  }

  tags = local.common_tags
}
