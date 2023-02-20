namespace FrogAlert.Monitoring.Configuration
{
    [Serializable]
    public class MonitoringConfiguration
    {
        [Serializable]
        public class Range
        {
            public float Minimum { get; set; }

            public float Maximum { get; set; }

            public bool WithinRange(float value)
            {
                return value >= Minimum && value < Maximum;
            }
        }

        [Serializable]
        public class AlertMessages
        {
            public string AllClear { get; set; }

            public string NoRecentData { get; set; }

            public string TempOutOfRange { get; set; }
            
            public string HumidityOutOfRange { get; set; }

            public string TempHumidityOutOfRange { get; set; }
        }

        public Range TempC { get; set; }

        public Range Humidity { get; set; }

        public float SecondsWithoutData { get; set; }

        public float SecondsBetweenAlerts { get; set; }

        public AlertMessages Messages { get; set; }
    }
}
