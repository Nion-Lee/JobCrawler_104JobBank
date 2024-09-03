namespace JobCrawler_104
{
    public class CrawlingSettings
    {
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
        public double IntervalInSeconds { get; init; }
    }
}