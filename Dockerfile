# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["GreaseMonkeyJournal.Api/GreaseMonkeyJournal.Api.csproj", "GreaseMonkeyJournal.Api/"]
COPY ["GreaseMonkeyJournal.Tests/GreaseMonkeyJournal.Tests.csproj", "GreaseMonkeyJournal.Tests/"]

# Restore dependencies
RUN dotnet restore "GreaseMonkeyJournal.Api/GreaseMonkeyJournal.Api.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/GreaseMonkeyJournal.Api"
RUN dotnet build "GreaseMonkeyJournal.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "GreaseMonkeyJournal.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "GreaseMonkeyJournal.dll"]
