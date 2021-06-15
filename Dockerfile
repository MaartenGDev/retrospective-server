FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

COPY Retro.Data/*.csproj /src/csproj-files/
COPY Retro.Web/*.csproj /src/csproj-files/

COPY . .
COPY Retro.Web/appsettings.json Retro.Web/appsettings.Development.json
RUN dotnet publish -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Retro.Web.dll"]