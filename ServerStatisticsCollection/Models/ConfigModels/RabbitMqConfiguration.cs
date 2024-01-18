namespace ServerStatisticsCollection.Models.ConfigModels
{
    public record RabbitMqConfiguration
    {
        public string? HostName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
