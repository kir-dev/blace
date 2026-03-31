FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src
COPY Blace.Server/Blace.Server.csproj Blace.Server/
COPY Blace.Shared/Blace.Shared.csproj Blace.Shared/
COPY Blace.Client/Blace.Client.csproj Blace.Client/
RUN dotnet restore Blace.Server/Blace.Server.csproj -p:RequiresAspNetWebAssets=true
COPY . .
WORKDIR /src/Blace.Server
RUN dotnet publish --no-restore ./Blace.Server.csproj -o /app/publish -p UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
USER $APP_UID
EXPOSE 8080
EXPOSE 8081
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blace.Server.dll"]
