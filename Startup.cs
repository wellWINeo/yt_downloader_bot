using Telegram.Bot;
using yt_downloader_bot.Services;

namespace yt_downloader_bot;

public class Startup
{
    private IConfiguration Configuration { get; }
    private BotConfig BotConfig { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        // read config from .json files and env vars
        var configBuilder = new ConfigurationBuilder();

        configBuilder.SetBasePath(Directory.GetCurrentDirectory());

        if (env.IsDevelopment())
            configBuilder.AddJsonFile("appsettings.Development.json", optional: true,
                reloadOnChange: true);
        else
            configBuilder.AddJsonFile("appsettings.json", optional: true,
                reloadOnChange: true);

        configBuilder.AddEnvironmentVariables();

        Configuration = configBuilder.Build();

        // parse config to BotConfig object
        BotConfig = Configuration.GetSection("BotConfig").Get<BotConfig>();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<ConfigureWebhook>();

        services.AddHttpClient("tgwebhook")
            .AddTypedClient<ITelegramBotClient>(httpClient =>
                new TelegramBotClient(BotConfig.Token, httpClient));

        services.AddScoped<HandleUpdateService>();

        services.AddControllers().AddNewtonsoftJson();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseRouting();
        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            var token = BotConfig.Token;
            endpoints.MapControllerRoute(name: "tgwebhook",
                                        pattern: $"bot/{token}",
                                        new { controller = "Webhook", action = "Post" });
            endpoints.MapControllers();
        });
    }
}
