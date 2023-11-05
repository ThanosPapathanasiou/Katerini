FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["source/", "source/"]
RUN dotnet restore "source/Katerini.Website/Katerini.Website.fsproj"
COPY . .
WORKDIR "/src/source/Katerini.Website"
RUN dotnet build "Katerini.Website.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Katerini.Website.fsproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Update package lists and install curl
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "Katerini.Website.dll"]
