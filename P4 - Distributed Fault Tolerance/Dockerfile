# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble AS base
USER $APP_UID
WORKDIR /src
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["P4 - Distributed Fault Tolerance/P4 - Distributed Fault Tolerance.csproj", "P4 - Distributed Fault Tolerance/"]
RUN dotnet restore "./P4 - Distributed Fault Tolerance/P4 - Distributed Fault Tolerance.csproj"
COPY . .
WORKDIR "/src/P4 - Distributed Fault Tolerance"
RUN dotnet build "./P4 - Distributed Fault Tolerance.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./P4 - Distributed Fault Tolerance.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "P4 - Distributed Fault Tolerance.dll"]