FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory inside the container
WORKDIR /app
COPY ["CubeLogic.TransactionsConverter.csproj", "./"]
RUN dotnet restore

# Copy the entire project directory and build the application
COPY . ./
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0

# Set the working directory for the runtime container
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build-env /out ./
COPY InputTransactions.csv ./
COPY config.json ./

USER $APP_UID
ENTRYPOINT ["dotnet", "CubeLogic.TransactionsConverter.dll"]
