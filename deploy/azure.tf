terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.14.0"
    }
  }
}

provider "azurerm" {
  features {}

  
}

# Resource group
resource "azurerm_resource_group" "rg" {
  name     = "dotnet-app-rg"
  location = "UK South"
}

# Storage account for input and output files
resource "azurerm_storage_account" "storage" {
  name                     = "dotnetappstorage"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

# Storage container for input files
resource "azurerm_storage_container" "input_files" {
  name                  = "inputfiles"
  storage_account_name  = azurerm_storage_account.storage.name
  container_access_type = "private"
}

# Storage container for output files
resource "azurerm_storage_container" "output_files" {
  name                  = "outputfiles"
  storage_account_name  = azurerm_storage_account.storage.name
  container_access_type = "blob"
}


# Event Grid Topic for input file changes
resource "azurerm_eventgrid_topic" "input_file_changes" {
  name                = "input-file-changes-topic"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}

# Event Subscription for WebJob
resource "azurerm_eventgrid_event_subscription" "webjob_subscription" {
  name                      = "webjob-subscription"
  scope                     = azurerm_eventgrid_topic.input_file_changes.id

  webhook_endpoint {
    url = "http://${azurerm_container_group.container.ip_address}"
  }

  included_event_types = ["Microsoft.Storage.BlobCreated", "Microsoft.Storage.BlobDeleted"]

  retry_policy {
    max_delivery_attempts = 3
    event_time_to_live = 1440
  }
}

# Event Subscription for Logic App
resource "azurerm_eventgrid_event_subscription" "logicapp_subscription" {
  name                      = "logicapp-subscription"
  scope                     = azurerm_eventgrid_topic.input_file_changes.id

  webhook_endpoint {
    url = azurerm_logic_app_workflow.file_change_email.access_endpoint
  }         
  included_event_types      = ["Microsoft.Storage.BlobCreated", "Microsoft.Storage.BlobDeleted"]
  
  retry_policy {
    max_delivery_attempts = 3
    event_time_to_live    = 1440
  }
}

# Azure Container Instance for the WebJob application
resource "azurerm_container_group" "container" {
  name                = "dotnet-webjob-container"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"

  container {
    name   = "dotnet-webjob"
    image  = "your-webjob-docker-image:latest"
    cpu    = "0.5"
    memory = "1.5"

    environment_variables = {
      AzureWebJobsStorage = azurerm_storage_account.storage.primary_connection_string
    }

    volume {
      name = "input-volume"
      mount_path = "/mnt/input"
   
      share_name          = azurerm_storage_container.input_files.name
      storage_account_name = azurerm_storage_account.storage.name
      storage_account_key  = azurerm_storage_account.storage.primary_access_key
   
    }

    volume {
      name = "output-volume"
      mount_path = "/mnt/output"
      share_name          = azurerm_storage_container.output_files.name
      storage_account_name = azurerm_storage_account.storage.name
      storage_account_key  = azurerm_storage_account.storage.primary_access_key
    
    }
  
  }

  restart_policy = "OnFailure"
}

# Logic App to send emails on input file changes
resource "azurerm_logic_app_workflow" "file_change_email" {
  name                = "file-change-email-logic-app"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  workflow_schema = jsonencode({
    "$schema" = "https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowDefinition.json#"
    "actions" = {
      "Send_an_email" = {
        "inputs" = {
          "body" = "File change detected in the input files."
          "subject" = "File Change Notification"
          "to" = "dynamic-email-list@from-config"
          "host" = {
            "connectionName" = "office365"
            "operationId" = "SendEmail"
            "connection" = {
              "name" = "@parameters('$connections')['office365']['connectionId']"
            }
          }
        }
        "runAfter" = {}
        "type" = "ApiConnection"
      }
    }
    "triggers" = {
      "When_a_blob_is_added_or_modified" = {
        "inputs" = {
          "parameters" = {
            "blobPath" = "inputfiles/*"
            "connection" = {
              "name" = "@parameters('$connections')['azureblob']['connectionId']"
            }
          }
          "recurrence" = {
            "frequency" = "Minute"
            "interval" = 5
          }
        }
        "metadata" = {
          "operationId" = "OnUpdatedFile"
          "connectionName" = "azureblob"
        }
        "type" = "ApiConnection"
      }
    }
  })
}


# Output for container public IP
output "container_ip" {
  value = azurerm_container_group.container.ip_address
}
