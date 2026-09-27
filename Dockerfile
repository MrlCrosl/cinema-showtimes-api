# Build stage: restore first (only project files) so source changes do not invalidate the restore layer.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Cinema.Domain/Cinema.Domain.csproj src/Cinema.Domain/
COPY src/Cinema.Application/Cinema.Application.csproj src/Cinema.Application/
COPY src/Cinema.Infrastructure/Cinema.Infrastructure.csproj src/Cinema.Infrastructure/
COPY src/Cinema.Api/Cinema.Api.csproj src/Cinema.Api/
RUN dotnet restore src/Cinema.Api/Cinema.Api.csproj

COPY src/ src/
RUN dotnet publish src/Cinema.Api/Cinema.Api.csproj -c Release --no-restore -o /app/publish

# Runtime stage: HTTP only on 8080, non-root user, SQLite file on a volume so data survives restarts.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Cinema="Data Source=/app/data/cinema.db"

RUN mkdir -p /app/data && chown $APP_UID:$APP_UID /app/data
VOLUME /app/data
EXPOSE 8080

COPY --from=build /app/publish ./
USER $APP_UID

ENTRYPOINT ["dotnet", "Cinema.Api.dll"]
