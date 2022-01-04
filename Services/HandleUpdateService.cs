using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeExplode.Converter;

namespace yt_downloader_bot.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;

    public HandleUpdateService(ITelegramBotClient botClient,
                            ILogger<HandleUpdateService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task HandleMessageAsync(Update update)
    {
        // routing to message handler
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
            _ => UnknownUpdateHandlerAsync(update)
        };

        // try to execute handler
        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    // regular/edited message handler
    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {messageType}", message.Type);
        if (message.Type != MessageType.Text)
            return;

        // routing by command
        var action = message.Text!.Split(' ')[0] switch
        {
            "/start" => StartupMessage(_botClient, message),
            "/help" => Usage(_botClient, message),
            _ => AnyMessageProcess(_botClient, message)
        };

        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {sentMessageId}", sentMessage.MessageId);

        // handler to send 'welcome' message
        static async Task<Message> StartupMessage(ITelegramBotClient bot, Message message)
        {
            // TODO: write more detailed welcome message
            const string startupMsg = "Welcome!";
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                text: startupMsg);
        }

        // handler for any message
        static async Task<Message> AnyMessageProcess(ITelegramBotClient bot, Message message)
        {
            // run video download/upload in new thread
            // to send http response to server & avoid duplicates
            var thread = new Thread(async () => await SendYoutubeVideo(bot, message));
            thread.Name = "YT_Downloader";
            thread.Start();

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                        text: $"Video download in progress...");
        }

        static async Task SendYoutubeVideo(ITelegramBotClient bot, Message message)
        {
            // youtube client
            var youtube = new YoutubeExplode.YoutubeClient();

            // get video's metadata
            YoutubeExplode.Videos.Video? metadata = null;
            try
            {
                metadata = await youtube.Videos.GetAsync(message.Text);
            }
            catch (ArgumentException)
            {
                await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                            text: "Invalid link provided!");
                return;
            }

            // download video in /tmp folder
            var path = @$"/tmp/{metadata?.Id}.mp4";

            await youtube.Videos.DownloadAsync(message.Text, path);

            // upload to telegram
            using FileStream fileStream = new(path, FileMode.Open,
                                            FileAccess.Read, FileShare.Read);

            try
            {
                await bot.SendVideoAsync(chatId: message.Chat.Id,
                                            video: new InputOnlineFile(fileStream, metadata?.Title));
            }
            catch (RequestException)
            {
                await bot.SendTextMessageAsync(chatId: message.Chat.Id, "Can't send video(");
            }
        }

        // handler for /help command
        static async Task<Message> Usage(ITelegramBotClient bot, Message message)
        {
            const string usage = "Usage:\n" +
                                 "/start  - send brief description of this bot\n" +
                                 "/help   - send this message\n" +
                                 "/<link> - download video by link and send to you.";

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: usage,
                                                  replyMarkup: new ReplyKeyboardRemove());
        }
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {updateType}", update.Type);
        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(Exception exception)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }

}