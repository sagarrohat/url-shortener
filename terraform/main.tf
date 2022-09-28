terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.0.0"
    }
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}

# Create a resource group
resource "azurerm_resource_group" "main" {
  name     = "urlshortener"
  location = "East US"
}

resource "azurerm_virtual_network" "main" {
  name                = "url-shortener-vnet"
  address_space       = ["10.7.29.0/24"]
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
}

resource "azurerm_subnet" "internal" {
  name                 = "url-shortener-internal-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.7.29.0/25"]
  service_endpoints    = ["Microsoft.Sql"]

  delegation {
    name = "delegation"

    service_delegation {
      name    = "Microsoft.ContainerInstance/containerGroups"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_redis_cache" "main" {
  name                = "url-shortener-cache"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  capacity            = 2
  family              = "C"
  sku_name            = "Standard"
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"

  redis_configuration {
  }
}

resource "azurerm_postgresql_server" "main" {
  name                = "url-shortener-db-server-1gxe"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  sku_name = "GP_Gen5_2"

  storage_mb                   = 5120
  backup_retention_days        = 7
  geo_redundant_backup_enabled = false
  auto_grow_enabled            = false

  administrator_login              = var.postgresql_server_user
  administrator_login_password     = var.postgresql_server_password
  version                          = "11"
  ssl_enforcement_enabled          = false
  ssl_minimal_tls_version_enforced = "TLSEnforcementDisabled"
}

resource "azurerm_postgresql_database" "main" {
  name                = var.postgresql_database_name
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_postgresql_server.main.name
  charset             = "UTF8"
  collation           = "English_United States.1252"
}

resource "azurerm_postgresql_virtual_network_rule" "postgresql_internal_network_rule" {
  name                = "postgresql-vnet-rule"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_postgresql_server.main.name
  subnet_id           = azurerm_subnet.internal.id
}


resource "azurerm_network_profile" "api_container_group_profile" {
  name                = "container-group-network-profile"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  container_network_interface {
    name = "container-network-interface"

    ip_configuration {
      name      = "container-ip-configuration"
      subnet_id = azurerm_subnet.internal.id
    }
  }
}

resource "azurerm_public_ip" "api_lb" {
  name                = "url-shortener-api-ld-public-ip"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  allocation_method   = "Static"
  sku                 = "Standard"
  domain_name_label   = "urlshortener"
}

resource "azurerm_lb" "api" {
  name                = "url-shortener-api-lb"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "Standard"

  frontend_ip_configuration {
    name                 = "lb-frontend-ip"
    public_ip_address_id = azurerm_public_ip.api_lb.id
  }
}

resource "azurerm_lb_backend_address_pool" "api" {
  loadbalancer_id = azurerm_lb.api.id
  name            = "url-shortener-api-lb-pool"
}

resource "azurerm_container_group" "api" {
  count               = var.api_instances_count
  name                = "url-shortener-container-group-${count.index}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  ip_address_type    = "Private"
  network_profile_id = azurerm_network_profile.api_container_group_profile.id
  os_type            = "Linux"

  container {
    name   = "url-shortener-api"
    image  = "rohatsagar/cloudcomp15_sose22"
    cpu    = "1"
    memory = "1.5"

    ports {
      port     = 80
      protocol = "TCP"
    }

    ports {
      port     = 443
      protocol = "TCP"
    }

    environment_variables = {
      "ASPNETCORE_ENVIRONMENT"               = "Production"
      "AppOptions__DatabaseConnectionString" = "Host=${azurerm_postgresql_server.main.fqdn};Port=5432;Username=${var.postgresql_server_user}@${azurerm_postgresql_server.main.fqdn};Password=${var.postgresql_server_password};Database=${var.postgresql_database_name};"
      "AppOptions__RedisConnectionString"    = azurerm_redis_cache.main.primary_connection_string
    }
  }
}

resource "azurerm_lb_backend_address_pool_address" "api" {
  count                   = var.api_instances_count
  name                    = "url-shortener-api-${count.index}-lb-pool-address"
  backend_address_pool_id = azurerm_lb_backend_address_pool.api.id
  virtual_network_id      = azurerm_virtual_network.main.id
  ip_address              = azurerm_container_group.api[count.index].ip_address
}

resource "azurerm_lb_probe" "api_lb_probe" {
  loadbalancer_id = azurerm_lb.api.id
  name            = "url-shortener-api-lb-probe"
  port            = 80
}

resource "azurerm_lb_rule" "api_lb_rule" {
  loadbalancer_id                = azurerm_lb.api.id
  name                           = "url-shortener-api-lb-rule"
  protocol                       = "Tcp"
  frontend_port                  = 80
  backend_port                   = 80
  backend_address_pool_ids       = [azurerm_lb_backend_address_pool.api.id]
  probe_id                       = azurerm_lb_probe.api_lb_probe.id
  frontend_ip_configuration_name = "lb-frontend-ip"
}
