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
    
## Setup

### 1. Clone the Repository
Clone this repository to your local machine to get started with the project.

```bash
git clone https://github.com/Bina28/AzureFunctions.git

### 2. Configuration
The project uses the following configurations which must be set in the Azure portal or your local environment:

Azure Storage Account Connection: Used for Blob and Queue storage.

Cosmos DB Connection: Stores order data.

Event Grid Endpoint and Key: Used to subscribe to and trigger events for order status changes.

Key Vault URI: For managing sensitive information such as API keys.

Here’s an example configuration in the local.settings.json:

json

{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "your-azure-storage-connection-string",
    "CosmosDBConnection": "your-cosmosdb-connection-string",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ConnString": "your-azure-storage-connection-string",
    "BlobConnection": "your-azure-storage-connection-string",
    "eventGridEndpoint": "your-event-grid-endpoint",
    "eventGridKey": "your-event-grid-key",
    "VaultUri": "your-keyvault-uri",
    "EventGrid_AllowAnonymousAccess": "true"
  }
}

###3. Deploying to Azure
You can deploy the functions to Azure using the Azure Functions Tools for Visual Studio or the Azure CLI.

Using Azure Functions Tools for Visual Studio:
Open the solution in Visual Studio.

Right-click the project and select Publish.

Follow the prompts to deploy the function to Azure.

Using Azure CLI:
Run az login to authenticate with your Azure account.

Run func azure functionapp publish <your-function-app-name> to deploy the project.

Triggers and Bindings
HTTP Trigger
Trigger: Receives HTTP requests to initiate the order placement.

Binding: Reads order data as JSON from the request body.

Queue Trigger
Trigger: Processes order details by reading from a queue (order-processing-queue).

Binding: Fetches order details from Cosmos DB using the order ID from the queue message.

Cosmos DB Input
Binding: Used to fetch order details based on the order ID stored in the queue message.

Blob Storage Output
Binding: Uploads the generated invoice text file to Azure Blob Storage under the invoices container.

Event Grid Trigger
Trigger: Subscribes to events from Cosmos DB related to order status changes and sends notifications to customers.

 📊 System Design Diagram

![image](https://github.com/user-attachments/assets/f530fd14-5816-425e-900f-b065d29442e3)

