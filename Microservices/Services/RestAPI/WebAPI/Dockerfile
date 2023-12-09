#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Services/RestAPI/WebAPI/WebAPI.csproj", "Services/RestAPI/WebAPI/"]
COPY ["Services/RestAPI/Business/Business.csproj", "Services/RestAPI/Business/"]
COPY ["Services/RestAPI/DataAccess/DataAccess.csproj", "Services/RestAPI/DataAccess/"]
COPY ["Services/RestAPI/Entities/Entities.csproj", "Services/RestAPI/Entities/"]
COPY ["Services/RestAPI/Core/Core.csproj", "Services/RestAPI/Core/"]
RUN dotnet restore "Services/RestAPI/WebAPI/WebAPI.csproj"
COPY . .
WORKDIR "/src/Services/RestAPI/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]