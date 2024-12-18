# Use the official .NET runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8002
EXPOSE 8003

# Use the .NET SDK as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ["Auth/Auth.csproj", "./"]
RUN dotnet restore "Auth.csproj"
COPY Auth/ .
RUN dotnet publish "Auth.csproj" -c Release -o /app/publish

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copy the self-signed certificate
COPY ../.certs/auth.pfx /https/auth.pfx

ENTRYPOINT ["dotnet", "Auth.dll"]
