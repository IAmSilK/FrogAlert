using System.ComponentModel.DataAnnotations;

namespace FrogAlert.Database.Models
{
    [Serializable]
    public class EnvironmentSnapshot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTimeOffset Time { get; set; }

        [Required]
        public float TempC { get; set; }

        [Required]
        public float Humidity { get; set; }
    }
}
