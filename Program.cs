namespace HTTPServer;

public class Program
{
    public static void Main(string[] args)
    {
        HTTPServer.Launch(new HTTPServerConfiguration
        {
            Port = 8080,
            Scheme = HTTPServerConfiguration.SchemeType.HTTPS_SECURED
        });

        // keep program alive
        while (true)
        {
            Console.ReadKey();
        }
    }
}