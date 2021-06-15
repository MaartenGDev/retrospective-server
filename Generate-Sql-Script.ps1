#!/usr/bin/env pwsh
dotnet ef migrations script -c RetroContext -p Retro.Data -s Retro.Web --idempotent -o migrations.sql