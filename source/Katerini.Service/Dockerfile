FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["source/", "source/"]
RUN dotnet restore "source/Katerini.Service/Katerini.Service.csproj"
COPY . .
WORKDIR "/src/source/Katerini.Service"
RUN dotnet build "Katerini.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Katerini.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Katerini.Service.dll"]
