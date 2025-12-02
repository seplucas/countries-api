FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["CountriesApp.sln", "./"]
COPY ["CountriesApp.Api/CountriesApp.Api.csproj", "CountriesApp.Api/"]
COPY ["CountriesApp.Application/CountriesApp.Application.csproj", "CountriesApp.Application/"]
COPY ["CountriesApp.Domain/CountriesApp.Domain.csproj", "CountriesApp.Domain/"]
COPY ["CountriesApp.Infrastructure/CountriesApp.Infrastructure.csproj", "CountriesApp.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "CountriesApp.Api/CountriesApp.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/CountriesApp.Api"
RUN dotnet build "CountriesApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CountriesApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CountriesApp.Api.dll"]
