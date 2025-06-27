FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["pion-api/pion-api.csproj", "pion-api/"]
RUN dotnet restore "pion-api/pion-api.csproj"
COPY . .
WORKDIR "/src/pion-api"
RUN dotnet build "pion-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pion-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pion-api.dll"]
