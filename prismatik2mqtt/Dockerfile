#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["prismatik2mqtt/prismatik2mqtt.csproj", "prismatik2mqtt/"]
COPY ["Lightpack/Lightpack.csproj", "Lightpack/"]
RUN dotnet restore "prismatik2mqtt/prismatik2mqtt.csproj"
COPY . .
WORKDIR "/src/prismatik2mqtt"
RUN dotnet build "prismatik2mqtt.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "prismatik2mqtt.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "prismatik2mqtt.dll"]