# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Stage 2: Create a lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Set environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Copy the compiled files from the build stage
COPY --from=build /app/out .

# Start the application
ENTRYPOINT ["dotnet", "QuizApp.dll"]


