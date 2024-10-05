FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install NativeAOT build prerequisites
RUN apt-get update && apt-get install -y --no-install-recommends clang zlib1g-dev

COPY ["src/TomiSoft.HighLoad.App/TomiSoft.HighLoad.App.csproj", "TomiSoft.HighLoad.App/"]
COPY src/. .
RUN dotnet restore "TomiSoft.HighLoad.App/TomiSoft.HighLoad.App.csproj"

WORKDIR "/src/TomiSoft.HighLoad.App"
RUN dotnet build "TomiSoft.HighLoad.App.csproj" -c Release -r linux-x64 -o /app/build

FROM build AS publish
RUN dotnet publish "TomiSoft.HighLoad.App.csproj" -c Release -r linux-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /tools /tools
RUN echo "./TomiSoft.HighLoad.App | tee ./log/log.jsonl" > ./run.sh
RUN chmod a+x ./run.sh
ENTRYPOINT ["sh", "./run.sh"]