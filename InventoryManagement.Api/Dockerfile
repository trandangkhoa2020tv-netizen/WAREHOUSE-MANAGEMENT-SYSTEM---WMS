FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY InventoryManagement.Api/InventoryManagement.Api.csproj InventoryManagement.Api/
COPY InventoryManagement.Shared/InventoryManagement.Shared.csproj InventoryManagement.Shared/
RUN dotnet restore InventoryManagement.Api/InventoryManagement.Api.csproj

COPY InventoryManagement.Api/ InventoryManagement.Api/
COPY InventoryManagement.Shared/ InventoryManagement.Shared/
RUN dotnet publish InventoryManagement.Api/InventoryManagement.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "InventoryManagement.Api.dll"]

