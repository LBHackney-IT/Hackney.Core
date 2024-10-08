FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.HealthCheck/Hackney.Core.HealthCheck.csproj ./Hackney.Core/Hackney.Core.HealthCheck/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.HealthCheck/Hackney.Core.Tests.HealthCheck.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.HealthCheck/
COPY ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/Hackney.Core.Testing.Shared.csproj ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/
COPY /nuget.config /root/.nuget/NuGet/NuGet.Config

# We mount secrets so they can't end up in logs or build layers.
# We chain both restore commands so we only make the token available
# once and don't store it elsewhere.
# see:
#   - https://docs.docker.com/reference/dockerfile/#arg
#   - https://docs.docker.com/compose/how-tos/use-secrets/ 
RUN --mount=type=secret,id=LBHPACKAGESTOKEN \
  export LBHPACKAGESTOKEN=$(cat /run/secrets/LBHPACKAGESTOKEN) && \
  dotnet restore ./Hackney.Core/Hackney.Core.HealthCheck/Hackney.Core.HealthCheck.csproj && \
  dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.HealthCheck/Hackney.Core.Tests.HealthCheck.csproj && \
  dotnet restore ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/Hackney.Core.Testing.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.HealthCheck/Hackney.Core.Tests.HealthCheck.csproj

CMD dotnet test Hackney.Core.Tests/Hackney.Core.Tests.HealthCheck
