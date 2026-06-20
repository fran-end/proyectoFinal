using System.Collections.Generic;

public class HttpRequest
{
    public string Method { get; set; } = "";
    
    public string Path { get; set; } = "/index.html";

    public Dictionary<string, string> QueryParams { get; set; } = new();

    public Dictionary<string, string> Headers { get; set; } = new();

    public string Body { get; set; } = "";

    public override string ToString()
    {
        return $"{Method} {Path} | QueryParams: {QueryParams.Count} | Headers: {Headers.Count} | BodyLength: {Body.Length}";
    }
}