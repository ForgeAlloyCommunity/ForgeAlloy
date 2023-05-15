# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /ForgeServerRegistryService
COPY ForgeServerRegistryService/*.csproj .
RUN dotnet restore
COPY ForgeServerRegistryService .
COPY ForgeServerRegistryService/Refs/Forge.dll .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "ForgeServerRegistryService.dll"]