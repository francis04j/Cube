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

# Create a non-root user
RUN useradd -m -u 1000 appuser

# Switch to the new user
USER appuser

# Check permissions for the app user (optional)
RUN ls -l /app && echo "Permissions for /app:"

# Create empty volumes for each argument path
VOLUME ["--mode=0755", "/app/input_file_path"] 
VOLUME ["--mode=0755", "/app/config_file_path"] 
VOLUME ["--mode=0755", "/app/output"]


# Copy the built application from the build stage
COPY --from=build-env /out ./

# Pass arguments as environment variables
ENV INPUT_PATH=/app/InputTransactions.csv
ENV CONFIG_PATH=/app/config.json
ENV OUTPUT_PATH=/app/output/output.csv

USER $APP_UID
ENTRYPOINT ["sh", "-c", "dotnet CubeLogic.TransactionsConverter.dll --input ${INPUT_PATH} --config ${CONFIG_PATH} --output ${OUTPUT_PATH}"]

# Example how to run 
# folder with input files is called input_file_path
# folder for output file is called output
# docker run -u root -v $(pwd)/input_file_path/inputTransactions.csv:/app/inputTransactions.csv -v $(pwd)/input_file_path/config.json:/app/input_file_path/config.json -v $(pwd)/output:/app/output transconv