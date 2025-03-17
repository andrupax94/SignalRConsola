using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using misFunciones;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

using System.Text.RegularExpressions;
using SignalRConsola;
using Newtonsoft.Json;

class Program
{
    private static PrinterService _printerService = new PrinterService();
    private static HubConnection _connection;
    private static string _selectedConnectionId; // Variable para almacenar la ID de conexión seleccionada
    private static Regex connectionIdRegex = new Regex(@"^[a-zA-Z0-9]{20,}$"); // Expresión regular para validar ID de conexión
    private static string guid;

    public static async Task Main(string[] args)
    {

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Seleccione una opción para conectarse:");
            Console.WriteLine("1. Usuario y Contraseña");
            Console.WriteLine("2. Autenticarse con Google");

            var opcion = Console.ReadKey().KeyChar;
            Console.Clear();

            switch (opcion)
            {
                case '1':
                    await AutenticacionUsuarioYContrasena();
                    break;

                case '2':
                    Console.WriteLine("Aun no Disponible.");
                    Console.WriteLine("Presione cualquier tecla para volver al menú principal...");
                    Console.ReadKey();
                    break;

                default:
                    Console.WriteLine("Opción no válida. Presione cualquier tecla para volver al menú principal...");
                    Console.ReadKey();
                    break;
            }
        }

    }
    private static async Task AutenticacionUsuarioYContrasena()
    {
        Console.Write("Ingrese su usuario: ");
        var usuario = Console.ReadLine();

        Console.Write("Ingrese su contraseña: ");
        var contrasena = Console.ReadLine();

        var httpClient = new HttpClient();
        var loginInfo = new { username = usuario, password = contrasena };
        var content = new StringContent(JsonConvert.SerializeObject(loginInfo), Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync("https://localhost:7001/logIn", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Autenticación exitosa: " + result);
                // Aquí podrías continuar con la lógica de conexión al hub SignalR
            }
            else
            {
                Console.WriteLine("Autenticación fallida. Presione cualquier tecla para volver al menú principal...");
                Console.ReadKey();
            }
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("No se pudo conectar. Presione cualquier tecla para volver al menú principal...");
            Console.ReadKey();
        }
        await Negociacion();
    }
    public static async Task Negociacion()
    {
        var appSettings = ReadAppSettings("./appsettings.json");
        guid = appSettings.ApplicationSettings.GUID;
        _selectedConnectionId = appSettings.ApplicationSettings.GUIDD;


        Console.WriteLine($"El GUID de la aplicación es: {guid}");
        Console.WriteLine("Iniciando cliente SignalR...");
        //var url = "wss://ws-eue5czgwbbbuard7.canadacentral-01.azurewebsites.net/chatHub"; // Reemplaza con tu URL de Azure SignalR
        var url = "wss://serviciointermedio.azurewebsites.net/chatHub"; // Reemplaza con tu URL de Azure SignalR
        //var url = "https://localhost:7001/chathub"; // Reemplaza con tu URL de Azure SignalR

        _connection = new HubConnectionBuilder()
            .WithUrl(url + $"?guid={guid}")
            .WithAutomaticReconnect()
            .Build();

        // Manejar mensajes recibidos desde el servidor
        _connection.On<string, string>("ReceiveMessage", (senderConnectionId, message) =>
        {
            if (!misFunciones.Json.IsValidJson(message, out JsonDocument jsonDoc))
            {
                Console.WriteLine("Mensaje no es un JSON válido.");
                return;
            }

            var instruccion = jsonDoc.RootElement.GetProperty("instruccion").GetString();
            JsonElement datosElement = jsonDoc.RootElement.GetProperty("datos");
            var mensajeJson = "";
            switch (instruccion.ToLower())
            {
                case "mensaje":
                    string datos = datosElement.GetString();
                    Console.WriteLine($"📩 Mensaje recibido: {datos}");
                    mensajeJson = misFunciones.Mensajes.GenerateMessaje("Mensaje Recibido");
                    _connection.InvokeAsync("SendMessageToClient", senderConnectionId, mensajeJson);
                    break;

                case "impresoras":
                    var printersJson = _printerService.GetInstalledPrinters();
                    mensajeJson = misFunciones.Mensajes.GeneratePrinterListRequest(printersJson);
                    _connection.InvokeAsync("SendMessageToClient", senderConnectionId, mensajeJson);
                    Console.WriteLine($"Lista de impresoras enviada: {printersJson}");
                    break;

                case "imprimir":

                    if (!misFunciones.Json.IsValidPrintData(datosElement, out string printerName, out string datosImprimir))
                    {
                        Console.WriteLine("Datos de impresión no válidos.");
                        return;
                    }
                    Printers pdfPrinter = new Printers();
                    if (pdfPrinter.PrintToPDF(datosImprimir, printerName))
                    {
                        mensajeJson = misFunciones.Mensajes.GenerateMessaje("Documento PDF impreso.");
                        _connection.InvokeAsync("SendMessageToClient", senderConnectionId, mensajeJson);
                        Console.WriteLine("Documento PDF impreso.");
                    }
                    else
                        Console.WriteLine("Error Al Imprimir Documento");
                    break;

                default:
                    Console.WriteLine("Instrucción no disponible.");
                    break;
            }
        });

        await _connection.StartAsync();
        Console.WriteLine($"🟢 Conectado con ID: {_connection.ConnectionId}");

        // Iniciar la tarea para manejar los mensajes desde la consola
        _ = Task.Run(() => HandleConsoleInput());

        // Mantener la conexión abierta
        while (true)
        {
            await Task.Delay(1000);
        }
    }
    static AppSettings ReadAppSettings(string filePath)
    {
        var json = File.ReadAllText(filePath);

        return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
    }
    static async Task HandleConsoleInput()
    {
        while (true)
        {
            Console.Write("Escribe un mensaje (o ID destinatario|mensaje): ");
            var input = Console.ReadLine();

            // Comprobar si el input es una ID de conexión válida utilizando la expresión regular
            if (connectionIdRegex.IsMatch(input))
            {
                _selectedConnectionId = input;
                Console.WriteLine($"ID de conexión seleccionada: {_selectedConnectionId}");
                continue;
            }
            else
            {
                await EnviarMensaje(input);
            }
        }
    }

    static async Task EnviarMensaje(string input)
    {
        if (!string.IsNullOrEmpty(_selectedConnectionId))
        {
            await _connection.InvokeAsync("SendMessageToClient", _selectedConnectionId, input);
        }
        else
        {
            Console.WriteLine("No se ha especificado un id de conexion");
        }
    }


}


