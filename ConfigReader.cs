using System;
using System.Collections.Generic;
using System.IO;

public class ConfigReader
{
    public int Puerto { get; private set; }
    public string RootFolder { get; private set; }

    public ConfigReader(string rutaConfig)
    {
        if (!File.Exists(rutaConfig))
            throw new FileNotFoundException($"No se encontró el archivo de configuración: {rutaConfig}");

        var valores = new Dictionary<string, string>();

        foreach (var linea in File.ReadAllLines(rutaConfig))
        {
            var lineaLimpia = linea.Trim();
            if (string.IsNullOrEmpty(lineaLimpia) || lineaLimpia.StartsWith("#"))
                continue;

            var partes = lineaLimpia.Split('=', 2);
            if (partes.Length != 2)
                continue;

            valores[partes[0].Trim()] = partes[1].Trim();
        }

        if (!valores.ContainsKey("puerto") || !int.TryParse(valores["puerto"], out int puertoParseado))
            throw new InvalidDataException("config.txt debe tener una línea 'puerto=NUMERO' válida.");

        Puerto = puertoParseado;

        if (!valores.ContainsKey("rootFolder"))
            throw new InvalidDataException("config.txt debe tener una línea 'rootFolder=CARPETA'.");

        RootFolder = Path.GetFullPath(valores["rootFolder"]);

        if (!Directory.Exists(RootFolder))
            throw new DirectoryNotFoundException($"La carpeta raíz no existe: {RootFolder}");
    }
}