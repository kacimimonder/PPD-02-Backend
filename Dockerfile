# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and all project files
COPY Backend/Backend.sln ./Backend/
COPY Backend/API.csproj ./Backend/
COPY Application/Application.csproj ./Application/
COPY Domain/Domain.csproj ./Domain/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/

# Restore dependencies
WORKDIR /src/Backend
RUN dotnet restore "Backend.sln"

# Copy the full source code
COPY Backend/ ./Backend/
COPY Application/ ./Application/
COPY Domain/ ./Domain/
COPY Infrastructure/ ./Infrastructure/

# Build and publish
WORKDIR /src/Backend
RUN dotnet build API.csproj --no-restore -c Release
RUN dotnet publish API.csproj --no-restore -c Release -o /app/publish


# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]
