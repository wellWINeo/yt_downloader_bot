# Overview
ASP.NET core based telegram bot with webhook to download videos from youtube and upload to tg.

Bot on Telegram: @yt_downloader_tg_bot

# Usage
Commands:

*   /start - send 'welcome' messae
*   /help - send message with commands (similar to this)
*   <youtube_link> - send plaint link to youtube video to 
download it.

# Configuration
Bot needs `Token` (telegram bot token), `Host`
(domain name). You can set this variables by 
`appsettings.${ENVIRONMENT}.json` (for production env just
`appsettings.json`). Or you can set it by envornment variables, for example:
```
$ set BotConfig__Token SomeExampleToke
$ set BotConfig__Host https://example.com
```

To set listen port you can use `launchSettings.json` in 
development mode and `ASPNETCORE_URLS` in production mode. 

# Notices

## Port in container
For docker container listen port specifies by `$PORT` 
variable and passes as `ASPNETCORE_URLS="http://*:$PORT"`
in Dockerfile.

## Youtube Shorts
Currently videos from YouTube Shorts couldn't be 
downloaded. Support already merged into YoutubeExplode 
library, so as soon as Nuget packages are updated, support 
should appear.