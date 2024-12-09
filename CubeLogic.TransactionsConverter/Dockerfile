FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CubeLogic.TransactionsConverter/CubeLogic.TransactionsConverter.csproj", "CubeLogic.TransactionsConverter/"]
RUN dotnet restore "CubeLogic.TransactionsConverter/CubeLogic.TransactionsConverter.csproj"
COPY . .
WORKDIR "/src/CubeLogic.TransactionsConverter"
RUN dotnet build "CubeLogic.TransactionsConverter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CubeLogic.TransactionsConverter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CubeLogic.TransactionsConverter.dll"]
