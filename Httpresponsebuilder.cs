using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

public static class HttpResponseBuilder
{
    public static readonly Dictionary<string, string> MimeTypes = new()
    {
        { ".html", "text/html; charset=utf-8" },
        { ".htm",  "text/html; charset=utf-8" },
        { ".css",  "text/css; charset=utf-8" },
        { ".js",   "application/javascript; charset=utf-8" },
        { ".json", "application/json; charset=utf-8" },
        { ".png",  "image/png" },
        { ".jpg",  "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif",  "image/gif" },
        { ".svg",  "image/svg+xml" },
        { ".ico",  "image/x-icon" },
        { ".txt",  "text/plain; charset=utf-8" },
    };

    public static string GetMimeType(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        return MimeTypes.TryGetValue(extension, out string? mime)
            ? mime
            : "application/octet-stream";
    }

    private static byte[] Comprimir(byte[] datos)
    {
        using var memoryStream = new MemoryStream();
        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
        {
            gzipStream.Write(datos, 0, datos.Length);
        }
        return memoryStream.ToArray();
    }

    public static void Send(
        NetworkStream stream,
        int statusCode,
        string statusText,
        byte[] bodyBytes,
        string contentType,
        Dictionary<string, string>? headersExtra = null)
    {
        byte[] bodyComprimido = Comprimir(bodyBytes);

        var headerBuilder = new StringBuilder();
        headerBuilder.Append($"HTTP/1.1 {statusCode} {statusText}\r\n");
        headerBuilder.Append($"Content-Type: {contentType}\r\n");
        headerBuilder.Append("Content-Encoding: gzip\r\n");
        headerBuilder.Append($"Content-Length: {bodyComprimido.Length}\r\n");
        headerBuilder.Append("Connection: close\r\n");

        if (headersExtra != null)
        {
            foreach (var kv in headersExtra)
            {
                headerBuilder.Append($"{kv.Key}: {kv.Value}\r\n");
            }
        }

        headerBuilder.Append("\r\n");

        byte[] headerBytes = Encoding.UTF8.GetBytes(headerBuilder.ToString());

        stream.Write(headerBytes, 0, headerBytes.Length);
        stream.Write(bodyComprimido, 0, bodyComprimido.Length);
        stream.Flush();
    }

    public static void SendText(
        NetworkStream stream,
        int statusCode,
        string statusText,
        string bodyTexto,
        string contentType = "text/html; charset=utf-8")
    {
        byte[] bodyBytes = Encoding.UTF8.GetBytes(bodyTexto);
        Send(stream, statusCode, statusText, bodyBytes, contentType);
    }

    public static bool SendFile(NetworkStream stream, string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
            return false;

        byte[] contenido = File.ReadAllBytes(rutaArchivo);
        string contentType = GetMimeType(rutaArchivo);
        Send(stream, 200, "OK", contenido, contentType);
        return true;
    }

    public static void SendNotFound(NetworkStream stream, string? rutaHtml404 = null)
    {
        string html = "<html><body><h1>404 - Recurso no encontrado</h1></body></html>";

        if (rutaHtml404 != null && File.Exists(rutaHtml404))
            html = File.ReadAllText(rutaHtml404);

        SendText(stream, 404, "Not Found", html);
    }
}