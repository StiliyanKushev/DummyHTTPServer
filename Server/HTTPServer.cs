using System.Net;
using System.Text;

namespace HTTPServer;

public struct HTTPServerConfiguration
{
    public enum SchemeType
    {
        HTTP_ONLY,
        HTTPS_SECURED
    };
    public int Port;
    public SchemeType Scheme;
}

public class HTTPServer
{
    private static readonly HttpListener _listener = new();
    
    /// <summary>
    ///  Launches the HTTP Proxy Server
    /// </summary>
    public static void Launch(HTTPServerConfiguration options)
    {
        Console.WriteLine(
            $"{options.Scheme} server launching at port \"{options.Port}\"");

        var scheme =
            options.Scheme == HTTPServerConfiguration.SchemeType.HTTP_ONLY
                ? "http"
                : "https";
        
        _listener.Prefixes.Add($"{scheme}://*:{options.Port}/");
        _listener.Start();
        Receive();
    }

    /// <summary>
    /// Called when the server is ready to receive more requests.
    /// </summary>
    private static void Receive()
    {
        _listener.BeginGetContext(ListenerCallback, _listener);
    }

    /// <summary>
    /// Main request handler function
    /// </summary>
    /// <param name="result"></param>
    private static async void ListenerCallback(IAsyncResult result)
    {
        if (!_listener.IsListening) return;
        
        var context = _listener.EndGetContext(result);
        var request = context.Request;
        var response = context.Response;

        var correctUri = new Uri(
            request.Url!.ToString().Replace(":8080", ""));

        Console.WriteLine("\n\nHTTP Request Info:");
        Console.WriteLine($"HttpMethod: {request.HttpMethod}");
        Console.WriteLine($"URL: {request.Url}, {correctUri}");
        Console.WriteLine($"Headers:\n{request.Headers}");
        
        // receive more incoming requests
        Receive();

        try
        {
            Stream? requestBodyStream = null;
            
            if (request.HasEntityBody)
            {
                // implement request body handling
                requestBodyStream = request.InputStream;
            }
            
            // make the request locally and retrieve the response info
            var requestResponse = await HTTPClient.MakeRequest(
                correctUri, 
                new HttpMethod(request.HttpMethod), 
                HTTPClient.CreateHeaders(request.Headers),
                requestBodyStream);
            
            // fire the response headers to the client
            response.Headers = HTTPClient.CreateResponseHeaders(
                requestResponse.Headers, requestResponse.ContentHeaders);
            response.StatusCode = (int)requestResponse.StatusCode;
            
            // if you don't remove content-length here we get .net errors.
            // this seems to be a bug in the bad design of .net.
            response.Headers.Remove("content-length");
            
            Console.WriteLine($"Response Headers:\n{response.Headers}");
            
            // return the response stream and close
            await requestResponse.ResponseStream.CopyToAsync(
                response.OutputStream);
            response.OutputStream.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}