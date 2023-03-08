using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace yt_downloader_bot.Services;

public class ConfigureWebhook : IHostedService
{
    private readonly ILogger<ConfigureWebhook> _logger;
    private readonly IServiceProvider _services;
    private readonly BotConfig _botConfig;

    public ConfigureWebhook(ILogger<ConfigureWebhook> logger,
                            IServiceProvider services,
                            IConfiguration configuration)
    {
        _logger = logger;
        _services = services;
        _botConfig = configuration.GetSection("BotConfig").Get<BotConfig>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider
            .GetRequiredService<ITelegramBotClient>();
        var webhookAddr = $"https://{_botConfig.Host}/bot/{_botConfig.Token}";
        _logger.LogInformation("Setting webhook: {WebhookAddr}", webhookAddr);
        await botClient.SetWebhookAsync(
            url: webhookAddr,
            allowedUpdates: Array.Empty<UpdateType>(),
            cancellationToken: cancellationToken
        );

    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider
            .GetRequiredService<ITelegramBotClient>();
        _logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}
