namespace RussianSitesStatus.Database;

public static class ConfigurationExtensions
{
    public static string GetConnectionString(this IConfiguration configuration)
    {
        var uri = new UriBuilder(configuration["DATABASE_URL"]);
        var connection = 
               $"Host={uri.Host};" + 
               $"Database={uri.Path.Remove(0,1)};" + 
               $"Username={uri.UserName};" + 
               $"Password={uri.Password};";
        
        if (uri.Host != "localhost" && uri.Host != "127.0.0.1")
        {
            connection += "sslmode=Require;Trust Server Certificate=true;";
        }

        return connection;
    }

    public static string GetDatabaseName(this IConfiguration configuration)
    {
        var uri = new UriBuilder(configuration["DATABASE_URL"]);
        return uri.Path.Remove(0, 1);
    }
}
