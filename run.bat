#!/bin/bash

concurrently "dotnet watch run --project ./Auth/Auth.csproj" "dotnet watch run --project ./Gateway/Gateway.csproj"