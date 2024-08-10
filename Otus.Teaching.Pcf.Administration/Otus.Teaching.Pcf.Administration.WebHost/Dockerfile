#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Otus.Teaching.Pcf.Administration/Otus.Teaching.Pcf.Administration.WebHost/Otus.Teaching.Pcf.Administration.WebHost.csproj", "Otus.Teaching.Pcf.Administration/Otus.Teaching.Pcf.Administration.WebHost/"]
COPY ["Otus.Teaching.Pcf.Administration/Otus.Teaching.Pcf.Administration.Core/Otus.Teaching.Pcf.Administration.Core.csproj", "Otus.Teaching.Pcf.Administration/Otus.Teaching.Pcf.Administration.Core/"]
COPY ["Otus.Teaching.Pcf.Administration/Otus.Teaching.Pcf.Administration.DataAccess/Otus.Teaching.Pcf.Administration.DataAccess.csproj", "Otus.Teaching.Pcf.Administration/Otus.Teaching.Pcf.Administration.DataAccess/"]
RUN dotnet restore "./Otus.Teaching.Pcf.Administration/Otus.Teaching.Pcf.Administration.WebHost/Otus.Teaching.Pcf.Administration.WebHost.csproj"
COPY . .
WORKDIR "/src/Otus.Teaching.Pcf.Administration/Otus.Teaching.Pcf.Administration.WebHost"
RUN dotnet build "./Otus.Teaching.Pcf.Administration.WebHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Otus.Teaching.Pcf.Administration.WebHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Otus.Teaching.Pcf.Administration.WebHost.dll"]