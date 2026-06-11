FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev
WORKDIR /src/ChatApp.API

COPY ChatApp.API/ChatApp.API.csproj ./
RUN dotnet restore ChatApp.API.csproj

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1
EXPOSE 8080

CMD ["dotnet", "watch", "--project", "ChatApp.API.csproj", "run", "--urls", "http://+:8080"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ChatApp.API/ChatApp.API.csproj ChatApp.API/
RUN dotnet restore ChatApp.API/ChatApp.API.csproj

COPY ChatApp.API/ ChatApp.API/
RUN dotnet publish ChatApp.API/ChatApp.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ChatApp.API.dll"]
