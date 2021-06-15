#!/usr/bin/env pwsh

$MigrationName = Read-Host -Prompt 'Migration Name'
Write-Host "Creating migration: '$MigrationName'"
dotnet ef migrations add $MigrationName -c RetroContext -p Retro.Data -s Retro.Web