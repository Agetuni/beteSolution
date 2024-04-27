namespace BuildingBlocks.Logging;
public class LogOptions
{
    public string Level { get; set; }
    public ElasticOptions Elastic { get; set; }

    public SentryOptions Sentry { get; set; }
    public FileOptions File { get; set; }
    public string LogTemplate { get; set; }
}
public class ElasticOptions
{
    public bool Enabled { get; set; }
    public string ElasticServiceUrl { get; set; }
    public string ElasticSearchIndex { get; set; }
}


public class SentryOptions
{
    public bool Enabled { get; set; }
    public string Dsn { get; set; }
    public string MinimumBreadcrumbLevel { get; set; }
    public string MinimumEventLevel { get; set; }
}

public class FileOptions
{
    public bool Enabled { get; set; }
    public string Path { get; set; }
    public string Interval { get; set; }
}