using BSUIRQueueTelegramBot.Data;
using BSUIRQueueTelegramBot.Repository;
using BSUIRQueueTelegramBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);
var botConfiguration = botConfigurationSection.Get<BotConfiguration>();

builder.Services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfig = botConfiguration;
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite("Data Source=records.db"));

builder.Services.AddScoped<UpdateHandlers>();
builder.Services.AddScoped<MessageCommands>();
builder.Services.AddScoped<RecordRepository>();
builder.Services.AddHostedService<ConfigureWebhook>();
builder.Services.AddHostedService<ResetQueueWorker>();

builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();



app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public class BotConfiguration
{
    public static readonly string Configuration = "BotConfiguration";

    public string BotToken { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
}