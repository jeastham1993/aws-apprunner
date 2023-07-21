FROM mcr.microsoft.com/dotnet/sdk:8.0-preview-alpine AS build-env
WORKDIR /App
COPY . .
RUN dotnet build "AppRuner.Api.NET6.csproj" -c Release -o /app/build --self-contained true
FROM build-env AS publish
RUN dotnet publish "AppRuner.Api.NET6.csproj" -c Release -o /app/publish --self-contained true

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview-alpine
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppRuner.Api.NET6.dll"]