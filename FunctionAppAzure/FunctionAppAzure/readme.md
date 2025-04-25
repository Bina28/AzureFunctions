# Azure Functions Order Processing System

## Overview

This project automates the order processing system for an online retail company using **Azure Functions**. 
The system handles the entire order lifecycle, from receiving and processing orders to sending notifications and updating the inventory. 
It demonstrates how to use different types of Azure Functions to build an event-driven architecture.

### Key Features:
- **HTTP Trigger**: Receives orders via POST requests and stores order data in Cosmos DB.
- **Queue Trigger**: Places order processing messages in an Azure Queue for further handling.
- **Cosmos DB**: Stores and updates order status.
- **HTTP Trigger**: Simulates order flow by updating order status and publishing events.
- **Timer Trigger**: Generates daily sales reports by querying Cosmos DB data.
- **Blob Storage**: Generates and stores invoices as `.txt` files in Azure Blob Storage.
- **Event Grid Trigger**: Subscribes to status change events from Cosmos DB and notifies customers of order updates.

## Requirements

- **Azure Functions** (with .NET isolated runtime)
- **Azure Storage Account** (for Blob and Queue storage)
- **Azure Cosmos DB** (for storing order data)
- **Azure Event Grid** (for subscribing to order status changes)
- **Azure Key Vault** (for storing sensitive information)

## ⚙️ Function Flow

1. **HTTP Trigger (`FunctionOrder`)**  
    - Receives a `POST` request with order details.  
    - Saves the order data to **Cosmos DB**.  
    - Sends a message to the **Order Processing Queue**.  
    - Returns a response containing the **Order ID**.

2. **Queue Trigger (`ProcessOrderQueue`)**  
    - Triggered by messages in the **Order Processing Queue**.  
    - Generates an **invoice** for the order.  
    - Uploads the invoice to **Azure Blob Storage**.

3. **Blob Trigger (`InvoiceNotificationFunction`)**  
    - Triggered when an **invoice** is uploaded to Blob Storage.  
    - Sends an **email** to the customer with the invoice attached.
    

4.  **HTTP Trigger (`SimulateOrderFlow`)**  
    - Receives a `POST` request with the order ID and status.  
    - Updates the order status in **Cosmos DB** if changed.  
    - Publishes a **status change event** if the status is updated.  
    
5.  **Event Grid Trigger (`NotifyOnOrderStatusChange`)**  
    - Subscribed to **Cosmos DB** change events.  
    - Listens for **order status changes**.  
    - Sends an email notification to the customer when their order status is updated.

6. **Cosmos DB Trigger (`UpdateInventoryOnOrderChange`)**  
    - Triggered by changes in the **Orders** container.  
    - Processes only orders with status `PaymentComplete`.  
    - Updates the **inventory quantity** for each item in the order in the **InventoryItem** container.

7. **Timer Trigger (`DailySalesReportFunction`)**  
    - Executes daily on a schedule.  
    - Generates a **sales report** using order data from **Cosmos DB**.
    
# Azure Functions Setup Guide

## Setup

### 1. Clone the Repository
Clone this repository to your local machine:

```bash
git clone https://github.com/Bina28/AzureFunctions.git
```

### 2. Configuration

Example Configuration in local.settings.json:

```json
{
"IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "<connection-string>",
    "CosmosDBConnection": "<cosmosdb-connection>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ConnString": "<connection-string>",
    "BlobConnection": "<connection-string>",
    "eventGridEndpoint": "<endpoint-url>",
    "eventGridKey": "<access-key>",
    "VaultUri": "<keyvault-uri>",
    "EventGrid_AllowAnonymousAccess": "true"
  }
}
```

### 3. Deployment Options
Visual Studio Method:
Open solution in VS 2022+

Right-click project → "Publish"
Select Azure target

### 4.  📊 System Design Diagram


![image](https://github.com/user-attachments/assets/f530fd14-5816-425e-900f-b065d29442e3)
