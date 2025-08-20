# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first to leverage caching
COPY *.csproj ./
RUN dotnet restore

# Copy everything else
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Bind to Render's port
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Run the DLL
ENTRYPOINT ["dotnet", "FinalAssignmentBackend.dll"]
