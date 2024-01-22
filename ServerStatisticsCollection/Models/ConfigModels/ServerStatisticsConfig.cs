namespace ServerStatisticsCollection.Models.ConfigModels
{
    public record ServerStatisticsConfig
    {
        public int SamplingIntervalSeconds { get; set; }
        public string ServerIdentifier { get; set; }
    }
}
