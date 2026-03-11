FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Services/PageCorrelationId.Api/PageCorrelationId.Api.csproj", "Services/PageCorrelationId.Api/"]
COPY ["Services/PageCorrelationId.Api.Utils/PageCorrelationId.Api.Utils.csproj", "Services/PageCorrelationId.Api.Utils/"]
COPY ["Services/PageCorrelationId.Application/PageCorrelationId.Application.csproj", "Services/PageCorrelationId.Application/"]
COPY ["Services/PageCorrelationId.Domain/PageCorrelationId.Domain.csproj", "Services/PageCorrelationId.Domain/"]
COPY ["Services/PageCorrelationId.Infrastructure/PageCorrelationId.Infrastructure.csproj", "Services/PageCorrelationId.Infrastructure/"]

RUN dotnet restore "Services/PageCorrelationId.Api/PageCorrelationId.Api.csproj"

COPY . .

RUN dotnet build "Services/PageCorrelationId.Api/PageCorrelationId.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Services/PageCorrelationId.Api/PageCorrelationId.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PageCorrelationId.Api.dll"]
