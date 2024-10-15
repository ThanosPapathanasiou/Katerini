# build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["source/", "source/"]
RUN dotnet restore "source/Katerini.Website/Katerini.Website.fsproj"
COPY . .
WORKDIR "/src/source/Katerini.Website"
RUN dotnet build "Katerini.Website.fsproj" -c Release -o /app/build

# publish to /app/publish
FROM build AS publish
RUN dotnet publish "Katerini.Website.fsproj" -c Release -o /app/publish /p:UseAppHost=false

# run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Katerini.Website.dll"]
