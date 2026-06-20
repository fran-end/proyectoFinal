using System;
using System.IO;

public class Logger
{
    private static readonly object _lock = new object();

    public static void Registrar(string ip, string metodo, string ruta)
    {
        string carpetaLogs = "logs";
        if (!Directory.Exists(carpetaLogs))
            Directory.CreateDirectory(carpetaLogs);

        string nombreArchivo = Path.Combine(carpetaLogs, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
        string linea = $"[{DateTime.Now:HH:mm:ss}] IP: {ip} | {metodo} {ruta}";

        lock (_lock)
        {
            File.AppendAllText(nombreArchivo, linea + Environment.NewLine);
        }
    }
}