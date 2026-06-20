using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

public static class HttpRequestParser
{
    public static HttpRequest Parse(string requestCrudo, NetworkStream? stream = null)
    {
        var request = new HttpRequest();

        string textoNormalizado = requestCrudo.Replace("\r\n", "\n");
        string[] lineas = textoNormalizado.Split('\n');

        if (lineas.Length == 0 || string.IsNullOrWhiteSpace(lineas[0]))
            throw new InvalidDataException("Request line vacía o inválida.");

        string[] partesRequestLine = lineas[0].Split(' ');
        if (partesRequestLine.Length < 2)
            throw new InvalidDataException($"Request line mal formada: '{lineas[0]}'");

        request.Method = partesRequestLine[0].ToUpperInvariant();
        string rutaCompleta = partesRequestLine[1];

        string path;
        string queryString = "";

        int signoPregunta = rutaCompleta.IndexOf('?');
        if (signoPregunta >= 0)
        {
            path = rutaCompleta.Substring(0, signoPregunta);
            queryString = rutaCompleta.Substring(signoPregunta + 1);
        }
        else
        {
            path = rutaCompleta;
        }

        request.Path = string.IsNullOrEmpty(path) ? "/" : path;
        request.QueryParams = ParseQueryString(queryString);

        int indiceLinea = 1;
        for (; indiceLinea < lineas.Length; indiceLinea++)
        {
            string linea = lineas[indiceLinea];

            if (string.IsNullOrEmpty(linea))
            {
                indiceLinea++;
                break;
            }

            int separador = linea.IndexOf(':');
            if (separador <= 0) continue;

            string clave = linea.Substring(0, separador).Trim();
            string valor = linea.Substring(separador + 1).Trim();
            request.Headers[clave] = valor;
        }

        if (!request.Headers.TryGetValue("Content-Length", out string? contentLengthStr)
            || !int.TryParse(contentLengthStr, out int contentLength)
            || contentLength <= 0)
        {
            return request;
        }

        string bodyParcial = indiceLinea < lineas.Length
            ? string.Join("\n", lineas, indiceLinea, lineas.Length - indiceLinea)
            : "";

        int bytesBodyParcial = Encoding.UTF8.GetByteCount(bodyParcial);

        if (bytesBodyParcial >= contentLength || stream == null)
        {
            request.Body = bodyParcial;
            return request;
        }

        byte[] bufferRestante = new byte[contentLength - bytesBodyParcial];
        int totalLeido = 0;
        while (totalLeido < bufferRestante.Length)
        {
            int n = stream.Read(bufferRestante, totalLeido, bufferRestante.Length - totalLeido);
            if (n == 0) break;
            totalLeido += n;
        }

        request.Body = bodyParcial + Encoding.UTF8.GetString(bufferRestante, 0, totalLeido);
        return request;
    }

    private static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var resultado = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(queryString))
            return resultado;

        string[] pares = queryString.Split('&');

        foreach (string par in pares)
        {
            if (string.IsNullOrWhiteSpace(par)) continue;

            int igual = par.IndexOf('=');
            string clave, valor;

            if (igual >= 0)
            {
                clave = Uri.UnescapeDataString(par.Substring(0, igual).Replace('+', ' '));
                valor = Uri.UnescapeDataString(par.Substring(igual + 1).Replace('+', ' '));
            }
            else
            {
                clave = Uri.UnescapeDataString(par.Replace('+', ' '));
                valor = "";
            }

            resultado[clave] = valor;
        }

        return resultado;
    }
}