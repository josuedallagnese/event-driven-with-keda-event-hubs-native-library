FROM mcr.microsoft.com/dotnet/runtime:6.0 as base

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/src/KedaWorker"
RUN dotnet build "KedaWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KedaWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KedaWorker.dll"]
