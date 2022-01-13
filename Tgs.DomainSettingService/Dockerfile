#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.DomainSettingService/Tgs.DomainSettingService.csproj", "Tgs.DomainSettingService/"]
COPY ["DomainSetting.Core/DomainSetting.Core.csproj", "DomainSetting.Core/"]
COPY ["DomainSetting.Repositories/DomainSetting.Repositories.csproj", "DomainSetting.Repositories/"]
RUN dotnet restore "Tgs.DomainSettingService/Tgs.DomainSettingService.csproj"
COPY . .
WORKDIR "/src/Tgs.DomainSettingService"
RUN dotnet build "Tgs.DomainSettingService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.DomainSettingService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.DomainSettingService.dll"]