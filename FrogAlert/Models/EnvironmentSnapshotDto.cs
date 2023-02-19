namespace FrogAlert.Models
{
    [Serializable]
    public class EnvironmentSnapshotDto
    {
        public DateTimeOffset Time { get; set; }

        public float TempC { get; set; }

        public float Humidity { get; set; }
    }
}
