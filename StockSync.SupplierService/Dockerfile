# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["StockSync.SupplierService/StockSync.SupplierService.csproj", "StockSync.SupplierService/"]
COPY ["StockSync.Shared/StockSync.Shared.csproj", "StockSync.Shared/"]
RUN dotnet restore "StockSync.SupplierService/StockSync.SupplierService.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/StockSync.SupplierService"
RUN dotnet build "StockSync.SupplierService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StockSync.SupplierService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockSync.SupplierService.dll"]
