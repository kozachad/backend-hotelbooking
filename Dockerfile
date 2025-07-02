# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY . ./
RUN dotnet restore

# Publish the application in Release mode
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published files from the build stage
COPY --from=build /app/out ./

# Start the application
ENTRYPOINT ["dotnet", "HotelBookingSystem.dll"] 
