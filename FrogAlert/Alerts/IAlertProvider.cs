namespace FrogAlert.Alerts
{
    public interface IAlertProvider
    {
        Task SendAlert(string message);
    }
}
