#!/usr/bin/env pwsh

Write-Host "Removing migration"
dotnet ef migrations remove -c RetroContext -p Retro.Data -s Retro.Web