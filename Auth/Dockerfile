# Use the official .NET runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8002
EXPOSE 8003

# Use the .NET SDK as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ["Infrastructure.sln", "./"]
COPY ["Auth/Auth.csproj", "./Auth/Auth.csproj"]
COPY ["Common/Common.csproj", "./Common/Common.csproj"]

RUN dotnet restore "Common/Common.csproj"
RUN dotnet restore "Auth/Auth.csproj"
COPY Auth/ ./Auth
COPY Common/ ./Common

RUN dotnet publish "Auth/Auth.csproj" -c Release -o /app/publish

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

COPY "Auth/Constants/EMailTemplate" "email-template"

# Copy the self-signed certificate
COPY ../.certs/auth.pfx /https/auth.pfx

ENTRYPOINT ["dotnet", "Auth.dll"]
