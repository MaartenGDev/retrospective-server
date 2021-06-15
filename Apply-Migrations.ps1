#!/usr/bin/env pwsh

Write-Host "Updating database with migrations"
dotnet ef database update -c RetroContext -p Retro.Data -s Retro.Web