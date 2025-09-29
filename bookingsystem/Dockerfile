# STAGE 1: build & publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopiera csproj först (exakta mappnamn med rätt versaler!)
COPY bookingsystem/*.csproj bookingsystem/
COPY Infrastructure/*.csproj Infrastructure/
COPY Data/*.csproj Data/

# Restore
RUN dotnet restore bookingsystem/*.csproj

# Kopiera resten av koden
COPY . .

# Publish
RUN dotnet publish bookingsystem/*.csproj -c Release -o /app/publish /p:UseAppHost=false

# STAGE 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
# Byt DLL-namn om ditt web-projekt heter något annat än 'bookingsystem'
ENTRYPOINT ["dotnet", "bookingsystem.dll"]
