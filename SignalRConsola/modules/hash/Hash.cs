using misFunciones;
using SignalRConsola.modules.signalR;

namespace SignalRConsola.modules.hash;
class Hash
{
    public void Ejecutar()
    {
        var continuar = true;
        while (continuar)
        {
            Console.Clear();

          

            Console.WriteLine(" ");
            Utilidades.CenterText(" Menu De Huellas Dactilares ");
            Console.WriteLine(" ");
            Console.WriteLine("1. Capturar Huella");
            Console.WriteLine("2. Convertir a condigo QR");
            Console.WriteLine("3. Registar Huella");
            Console.WriteLine("4. Verificar Huella");
            Console.WriteLine("0. Salir");
            Console.Write("Seleccione una opción: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "3":
                    RegistrarHuella();
                    break;
                case "2":
                   
                    break;
                case "1":
                    
                    break;
                case "0":
                    continuar = false;
                    break;
                case "4":

                    break;
                default:
                    Console.WriteLine("Opción no válida. Intente de nuevo.");
                    Console.ReadKey();
                    break;
            }
        }
    }
    private void RegistrarHuella() {
        Console.WriteLine("Registrar huella");
        Console.ReadKey();
    }


}