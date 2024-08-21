#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS builder

ARG BuildConfiguration=Release

WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.DomainSettingService/Tgs.DomainSettingService.csproj", "Tgs.DomainSettingService/"]
COPY ["DomainSetting.Core/DomainSetting.Core.csproj", "DomainSetting.Core/"]
COPY ["DomainSetting.Repositories/DomainSetting.Repositories.csproj", "DomainSetting.Repositories/"]
RUN dotnet restore "Tgs.DomainSettingService/Tgs.DomainSettingService.csproj"
COPY . .
RUN dotnet publish "Tgs.DomainSettingService.csproj" -c $BuildConfiguration -o /app

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Tgs.DomainSettingService.dll"]