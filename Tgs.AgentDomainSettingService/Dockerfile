#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.AgentDomainSettingService/Tgs.AgentDomainSettingService.csproj", "Tgs.AgentDomainSettingService/"]
COPY ["AgentDomainSetting.Repositories/AgentDomainSetting.Repositories.csproj", "AgentDomainSetting.Repositories/"]
COPY ["AgentDomainSetting.Core/AgentDomainSetting.Core.csproj", "AgentDomainSetting.Core/"]
RUN dotnet restore "Tgs.AgentDomainSettingService/Tgs.AgentDomainSettingService.csproj"
COPY . .
WORKDIR "/src/Tgs.AgentDomainSettingService"
RUN dotnet build "Tgs.AgentDomainSettingService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.AgentDomainSettingService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.AgentDomainSettingService.dll"]