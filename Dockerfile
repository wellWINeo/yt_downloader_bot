# build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

COPY ./* ./
RUN dotnet restore

RUN dotnet publish -c release -o /app --no-restore

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./

RUN apt-get update && apt-get install -y ffmpeg

# ENV ASPNETCORE_URLS="http://*:$PORT"
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT [ "dotnet", "yt_downloader_bot.dll" ]