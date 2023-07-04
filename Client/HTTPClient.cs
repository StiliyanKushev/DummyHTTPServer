using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;

namespace HTTPServer;

public struct HTTPResponse
{
    public HttpContentHeaders ContentHeaders;
    public HttpResponseHeaders Headers;
    public HttpStatusCode StatusCode;
    public Stream ResponseStream;
}

public class HTTPClient
{
    private static readonly HttpClient _httpClient = new ();

    /// <summary>
    /// Makes an HTTP(s) request of any type and returns the
    /// response along with the response body stream.
    /// </summary>
    public static async Task<HTTPResponse> MakeRequest(
        Uri url,
        HttpMethod method,
        HttpRequestHeaders headers,
        Stream? requestBodyStream)
    {
        var request = new HttpRequestMessage(method, url);

        if (requestBodyStream != null)
        {
            request.Content = new StreamContent(requestBodyStream);
        }

        foreach (var header in headers)
        {   
            request.Headers.Add(header.Key, header.Value);
        }
        
        var response = await _httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead);

        return new HTTPResponse
        {
            ContentHeaders = response.Content.Headers,
            Headers = response.Headers,
            StatusCode = response.StatusCode,
            ResponseStream = await response.Content.ReadAsStreamAsync()
        };
    }

    /// <summary>
    /// Converts standard request headers from NameValueCollection
    /// to HttpRequestHeaders.
    /// </summary>
    public static HttpRequestHeaders CreateHeaders(
        NameValueCollection requestHeaders)
    {
        var headers = new HttpRequestMessage().Headers;
        
        foreach (var key in requestHeaders.AllKeys)
        {
            var values = requestHeaders.GetValues(key) 
                         ?? Array.Empty<string>();

            foreach (var value in values)
            {
                headers.TryAddWithoutValidation(key!, value);
            }
        }

        return headers;
    }

    /// <summary>
    /// Converts HttpResponseHeaders to WebHeaderCollection.
    /// </summary>
    public static WebHeaderCollection CreateResponseHeaders(
        HttpResponseHeaders responseHeaders,
        HttpContentHeaders contentHeaders)
    {
        var webHeaders = new WebHeaderCollection();

        foreach (
            var (headerName, headerValues) 
            in responseHeaders)
        {
            foreach (var value in headerValues)
            {
                webHeaders.Add(headerName, value);
            }
        }
        
        foreach (
            var (headerName, headerValues) 
            in contentHeaders)
        {
            foreach (var value in headerValues)
            {
                webHeaders.Add(headerName, value);
            }
        }

        return webHeaders;
    }
}