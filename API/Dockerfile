FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY src/RussianSitesStatus/RussianSitesStatus.csproj /app
RUN dotnet restore
COPY ["src/RussianSitesStatus", "/app"]

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

CMD dotnet RussianSitesStatus.dll