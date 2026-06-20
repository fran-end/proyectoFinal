using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.IO;

ConfigReader config;
try
{
    config = new ConfigReader("config.txt");
}
catch (Exception ex)
{
    Console.WriteLine($"Error al leer config.txt: {ex.Message}");
    return;
}

Console.WriteLine($"Iniciando servidor en puerto {config.Puerto}, sirviendo desde {config.RootFolder}");

Socket socketEscucha = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

try
{
    IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, config.Puerto);
    socketEscucha.Bind(endpoint);
    socketEscucha.Listen(100);

    Console.WriteLine("Servidor escuchando. Esperando conexiones...");

    while (true)
    {
        Socket clienteSocket = socketEscucha.Accept(); // bloqueante
        Task.Run(() => ProcesarCliente(clienteSocket, config));
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error fatal en el servidor: {ex.Message}");
}
finally
{
    socketEscucha.Close();
}


void ProcesarCliente(Socket clienteSocket, ConfigReader config)
{
    try
    {
        IPEndPoint clienteEndpoint = (IPEndPoint)clienteSocket.RemoteEndPoint;
        Console.WriteLine($"Cliente conectado: {clienteEndpoint.Address}");

        using (NetworkStream stream = new NetworkStream(clienteSocket))
{
    byte[] buffer = new byte[4096];
    int bytesLeidos = stream.Read(buffer, 0, buffer.Length);

    if (bytesLeidos == 0)
    {
        return;
    }

    string requestCrudo = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);

    Console.WriteLine("----- REQUEST RECIBIDO -----");
    Console.WriteLine(requestCrudo);
    Console.WriteLine("-----------------------------");

    // aca juli conectas el HttpRequestParser.cs
    HttpRequest request = HttpRequestParser.Parse(requestCrudo, stream);

    Logger.Registrar(clienteEndpoint.Address.ToString(), request.Method, request.Path);

    if (request.QueryParams.Count > 0)
{
    foreach (var kv in request.QueryParams)
    {
        Console.WriteLine($"Query param: {kv.Key} = {kv.Value}");
        Logger.Registrar(clienteEndpoint.Address.ToString(), request.Method, $"{request.Path}?{kv.Key}={kv.Value}");
    }
}

    if (request.Method == "GET")
    {
        string rutaRelativa = request.Path == "/" ? "/index.html" : request.Path;
        string rutaArchivo = Path.Combine(config.RootFolder, rutaRelativa.TrimStart('/'));

        bool enviado = HttpResponseBuilder.SendFile(stream, rutaArchivo);

        if (!enviado)
        {
            HttpResponseBuilder.SendNotFound(stream, Path.Combine(config.RootFolder, "404.html"));
        }
    }
    else if (request.Method == "POST")
    {
        Console.WriteLine($"Body recibido por POST: {request.Body}");
        HttpResponseBuilder.SendText(stream, 200, "OK", "<h1>POST recibido</h1>");
    }
}
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error procesando cliente: {ex.Message}");
    }
    finally
    {
        clienteSocket.Close();
    }
}