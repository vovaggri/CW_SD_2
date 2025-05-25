FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY FileStoringService/*.csproj ./FileStoringService/
RUN dotnet restore FileStoringService/FileStoringService.csproj
COPY . .
WORKDIR /src/FileStoringService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FileStoringService.dll"]