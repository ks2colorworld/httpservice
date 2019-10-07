# https://docs.microsoft.com/ko-kr/aspnet/core/host-and-deploy/docker/visual-studio-tools-for-docker?view=aspnetcore-3.0
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 59518
EXPOSE 44364

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY HttpService/HttpService.csproj HttpService/
RUN dotnet restore HttpService/HttpService.csproj
COPY . .
WORKDIR /src/HttpService
RUN dotnet build HttpService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish HttpService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HttpService.dll"]