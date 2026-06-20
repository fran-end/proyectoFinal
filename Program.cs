using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

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

        Logger.Registrar(clienteEndpoint.Address.ToString(), "GET", "/ (sin parsear aún)");

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

    // se calcula longitud del body
    string body = "Hola, mundo!";
    byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

    string headers = "HTTP/1.1 200 OK\r\n" +
                      $"Content-Length: {bodyBytes.Length}\r\n" +
                      "Content-Type: text/plain\r\n" +
                      "Connection: close\r\n" +
                      "\r\n";

    byte[] headerBytes = Encoding.UTF8.GetBytes(headers);

    stream.Write(headerBytes, 0, headerBytes.Length);
    stream.Write(bodyBytes, 0, bodyBytes.Length);
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