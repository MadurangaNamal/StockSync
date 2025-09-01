FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["StockSync.ItemService/StockSync.ItemService.csproj", "StockSync.ItemService/"]
COPY ["StockSync.Shared/StockSync.Shared.csproj", "StockSync.Shared/"]
RUN dotnet restore "StockSync.ItemService/StockSync.ItemService.csproj"

COPY . .
WORKDIR "/src/StockSync.ItemService"
RUN dotnet build "StockSync.ItemService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StockSync.ItemService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockSync.ItemService.dll"]
