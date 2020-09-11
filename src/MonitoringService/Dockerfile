#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["src/MonitoringService/MonitoringService.csproj", "src/MonitoringService/"]
RUN dotnet restore "src/MonitoringService/MonitoringService.csproj"
COPY . .
WORKDIR "/src/src/MonitoringService"
RUN dotnet build "MonitoringService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MonitoringService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MonitoringService.dll"]