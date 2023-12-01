using BSUIRQueueTelegramBot.Repository;

namespace BSUIRQueueTelegramBot.Services
{
    public class ResetQueueWorker : BackgroundService
    {
        private readonly ILogger<ResetQueueWorker> _logger;
        private readonly DayOfWeek RESET_DAY= DayOfWeek.Friday;
        public IServiceProvider Services { get; }
        private DateTime LastRestartDate;
        

        public ResetQueueWorker(ILogger<ResetQueueWorker> logger,IServiceProvider services)
        {
            LastRestartDate = DateTime.UtcNow.AddHours(3);
            Services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("SQL RESETTER running at: {time}", DateTime.UtcNow);
                DateTime localDate= DateTime.UtcNow.AddHours(3);
                if(localDate.Date.DayOfWeek == RESET_DAY && LastRestartDate<=localDate)
                {
                    using (var scope = Services.CreateScope())
                    {
                        var repositoryService =
                            scope.ServiceProvider
                                .GetRequiredService<RecordRepository>();

                        repositoryService.ClearQueue();
                    }
                    LastRestartDate = localDate.AddDays(1);
                    _logger.LogWarning("SQL RESETTER HAVE JUST RESETTED QUERY at : {time}", DateTime.UtcNow);
                }
                await Task.Delay(1000*60, stoppingToken);
            }
        }
    }
}
