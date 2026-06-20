using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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

// Como estamos en top-level statements, los métodos se declaran al final del archivo
void ProcesarCliente(Socket clienteSocket, ConfigReader config)
{
    try
    {
        IPEndPoint clienteEndpoint = (IPEndPoint)clienteSocket.RemoteEndPoint;
        Console.WriteLine($"Cliente conectado: {clienteEndpoint.Address}");

        Logger.Registrar(clienteEndpoint.Address.ToString(), "GET", "/ (sin parsear aún)");

        using (NetworkStream stream = new NetworkStream(clienteSocket))
        {
            // PASO SIGUIENTE: leer bytes del stream y parsear el request HTTP
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