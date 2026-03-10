# Use official .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy from build stage
COPY --from=build /app/out .

# Expose port 80
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "article-service.dll"]