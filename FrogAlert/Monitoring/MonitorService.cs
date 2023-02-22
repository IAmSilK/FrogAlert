using FrogAlert.Alerts;
using FrogAlert.Alerts.SMS;
using FrogAlert.Database;
using FrogAlert.Monitoring.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FrogAlert.Monitoring
{
    public class MonitorService : BackgroundService
    {
        private readonly ILogger<MonitorService> _logger;
        private readonly FrogAlertDbContext _dbContext;
        private readonly MonitoringConfiguration _configuration;
        private readonly IAlertProvider[] _alertProviders;

        private DateTimeOffset _lastAlertTime = DateTimeOffset.MinValue;
        private bool _lastCheckAlert = false;

        public MonitorService(ILogger<MonitorService> logger,
            FrogAlertDbContext dbContext,
            IConfiguration configuration,
            SMSAlertProvider smsAlertProvider)
        {
            _logger = logger;
            _dbContext = dbContext;

            _alertProviders = new IAlertProvider[]
            {
                smsAlertProvider
            };

            _configuration = configuration.GetSection("Monitoring").Get<MonitoringConfiguration>();
        }

        private async Task SendAlertAsync(string message, bool ignoreLastAlertTime = false)
        {
            if (!ignoreLastAlertTime)
            {
                if ((DateTime.UtcNow - _lastAlertTime).TotalSeconds < _configuration.SecondsBetweenAlerts)
                {
                    return;
                }

                _lastAlertTime = DateTimeOffset.UtcNow;
            }

            _logger.LogInformation("Sending alert:\n{AlertMessage}", message);

            foreach (var alertProvider in _alertProviders)
            {
                try
                {
                    await alertProvider.SendAlert(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while sending alert via alert provider.");
                }
            }
        }

        private async Task CheckEnvironmentAsync()
        {
            var checkAlert = false;

            var latestSnapshot = await _dbContext.Environment.OrderByDescending(x => x.Time).FirstAsync();

            if ((DateTimeOffset.UtcNow - latestSnapshot.Time).TotalSeconds > _configuration.SecondsWithoutData)
            {
                // No recent data. Send alert.

                await SendAlertAsync(string.Format(_configuration.Messages.NoRecentData, latestSnapshot.TempC, latestSnapshot.Humidity, latestSnapshot.Time));
                
                checkAlert = true;
            }
            else if (!_configuration.TempC.WithinRange(latestSnapshot.TempC) ||
                     !_configuration.Humidity.WithinRange(latestSnapshot.Humidity))
            {
                // Temp or humidity out of range. Send alert.

                var tempOutOfBounds = !_configuration.TempC.WithinRange(latestSnapshot.TempC);
                var humidityOutOfBounds = !_configuration.Humidity.WithinRange(latestSnapshot.Humidity);

                string messageFormat;

                if (tempOutOfBounds && humidityOutOfBounds)
                {
                    messageFormat = _configuration.Messages.TempHumidityOutOfRange;
                }
                else if (tempOutOfBounds)
                {
                    messageFormat = _configuration.Messages.TempOutOfRange;
                }
                else
                {
                    messageFormat = _configuration.Messages.HumidityOutOfRange;
                }

                await SendAlertAsync(string.Format(messageFormat,
                    latestSnapshot.TempC, latestSnapshot.Humidity, latestSnapshot.Time));

                checkAlert = true;
            }

            if (!checkAlert && _lastCheckAlert)
            {
                await SendAlertAsync(string.Format(_configuration.Messages.AllClear,
                    latestSnapshot.TempC, latestSnapshot.Humidity, latestSnapshot.Time), true);
            }

            _lastCheckAlert = checkAlert;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);

                try
                {
                    await CheckEnvironmentAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred when checking environment conditions.");
                }
            }
        }
    }
}
