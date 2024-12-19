# Use the official .NET runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8001

# Use the .NET SDK as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["Infrastructure.sln", "./"]
COPY ["Gateway/Gateway.csproj", "./Gateway/Gateway.csproj"]
COPY ["Common/Common.csproj", "./Common/Common.csproj"]

RUN dotnet restore "Common/Common.csproj"
RUN dotnet restore "Gateway/Gateway.csproj"

COPY Gateway/ ./Gateway
COPY Common/ ./Common

RUN dotnet publish "Gateway/Gateway.csproj" -c Release -o /app/publish

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copy the self-signed certificate
COPY ../.certs/auth.pfx /https/auth.pfx

ENTRYPOINT ["dotnet", "Gateway.dll"]
