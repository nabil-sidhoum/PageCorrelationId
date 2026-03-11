FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Websites/PageCorrelationId.Site/PageCorrelationId.Site.csproj", "Websites/PageCorrelationId.Site/"]

RUN dotnet restore "Websites/PageCorrelationId.Site/PageCorrelationId.Site.csproj"

COPY . .

RUN dotnet build "Websites/PageCorrelationId.Site/PageCorrelationId.Site.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Websites/PageCorrelationId.Site/PageCorrelationId.Site.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PageCorrelationId.Site.dll"]
