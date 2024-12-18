# BeyondTrustConnector

[![Docker Image CI](https://github.com/FrodeHus/BeyondTrustConnector/actions/workflows/docker-image.yml/badge.svg)](https://github.com/FrodeHus/BeyondTrustConnector/actions/workflows/docker-image.yml)
[![.NET](https://github.com/FrodeHus/BeyondTrustConnector/actions/workflows/dotnet.yml/badge.svg)](https://github.com/FrodeHus/BeyondTrustConnector/actions/workflows/dotnet.yml)

## Description

This repository contains a function app running as a [container](https://hub.docker.com/r/frodehus/beyondtrustconnector) that pulls data from the [BeyondTrust API](https://www.beyondtrust.com/docs/privileged-remote-access/how-to/integrations/api/reporting/index.htm) and uploads it to Azure Sentinel using data collector ingestion endpoints. It also includes deployment templates for creating all necessary resources.

Uses function app to pull reports from BeyondTrust API and push to a Azure Log Analytics Workspace.

Requires:
- BeyondTrust API enabled
- BeyondTrust API credentials

## Install

Pre-requisites:
- Existing Log Analytics Workspace
- Key Vault with BeyondTrust API credentials secret

1. Update `main.bicepparam` to match your environment.
2. Run the following command to deploy the resources:
   ```sh
   az deployment group create --resource-group <group-name> --template-file main.bicep --parameters main.bicepparam
   ```

## Resources Deployed

This deployment will create:
- Data Collection Endpoint
- Data Collection Rules
- Custom tables for Azure Log Analytics Workspace (LAW)
- ASIM parsers
- Azure Function App
  - Managed identity with role assignments to read Key Vault secrets and query the workspace

### Custom Tables

The deployment creates the following custom tables in Azure Log Analytics Workspace:

- **BeyondTrustAccessSession_CL**: Stores session data from BeyondTrust, including session details, timestamps, and participants. This allows for analysis of session activities and user interactions.

- **BeyondTrustEvents_CL**: Contains audit and authentication logs capturing events such as logins, logouts, and configuration changes. Useful for compliance reporting and monitoring security-related events.

- **BeyondTrustVaultActivity_CL**: Holds information about password vault actvitity such as create/update/usage of passwords.

These custom tables provide valuable insights by enabling advanced querying and analytics within Azure Sentinel, helping to detect anomalies and potential security threats based on the data collected from BeyondTrust.

### ASIM Parsers

The following ASIM parsers are deployed:

- **vimAuthenticationBeyondTrust**: Follows the `Authentication` schema to integrate with the `imAuthentication` unified parser to provide authentication events.
- **vimAuditBeyondTrust**: Follows the `AuditEvent` schema to integrate with the `imAuditEvent` unified parser to provide audit events.

## What It Provides

- Automated data collection from BeyondTrust API
- Integration with Azure Sentinel for advanced threat detection and response
- Customizable deployment to fit your specific environment

