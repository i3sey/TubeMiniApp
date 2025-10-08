FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TubeMiniApp.API/TubeMiniApp.API.csproj", "TubeMiniApp.API/"]
RUN dotnet restore "TubeMiniApp.API/TubeMiniApp.API.csproj"
COPY . .
WORKDIR "/src/TubeMiniApp.API"
RUN dotnet build "TubeMiniApp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TubeMiniApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TubeMiniApp.API.dll"]
