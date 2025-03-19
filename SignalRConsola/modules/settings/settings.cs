namespace SignalRConsola.modules.settings
{
    class Settings
    {
        public string FilePath { get; set; } = "./modules/settings/appsettings.json";

        public AppSettings ReadAppSettings()
        {
            var json = File.ReadAllText(FilePath);
            return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
        }

        public void ChangeSettings(AppSettings appSettings)
        {
            // Aplicar color de fondo y texto si están definidos
            if (Enum.TryParse<ConsoleColor>(appSettings.ApplicationSettings.DefaultConfigurations.ThemeColor, out var backgroundColor))
            {
                Console.BackgroundColor = backgroundColor;
            }

            if (Enum.TryParse<ConsoleColor>(appSettings.ApplicationSettings.DefaultConfigurations.TextColor, out var textColor))
            {
                Console.ForegroundColor = textColor;
            }

            Console.Clear(); // Limpia la consola para aplicar el nuevo color de fondo

            // Aplicar título si está definido
            if (!string.IsNullOrEmpty(appSettings.ApplicationSettings.DefaultConfigurations.Title))
            {
                Console.Title = appSettings.ApplicationSettings.DefaultConfigurations.Title;
            }

            // Aplicar tamaño de la ventana basado en la configuración
            switch (appSettings.ApplicationSettings.DefaultConfigurations.WindowSize?.ToLower())
            {
                case "normal":
                    Console.SetWindowSize(80, 25); // Tamaño normal (por defecto)
                    Console.SetBufferSize(80, 25);
                    break;
                case "medium":
                    Console.SetWindowSize(96, 30); // Tamaño medio-grande
                    Console.SetBufferSize(96, 30);
                    break;
                case "large":
                    Console.SetWindowSize(120, 37); // Tamaño grande
                    Console.SetBufferSize(120, 37);
                    break;
                default:
                    Console.SetWindowSize(80, 25); // Tamaño por defecto si no coincide con las opciones
                    Console.SetBufferSize(80, 25);
                    break;
            }

            // Aplicar visibilidad del cursor
            if (appSettings.ApplicationSettings.DefaultConfigurations.CursorVisibility?.ToLower() == "hidden")
            {
                Console.CursorVisible = false;
            }
            else
            {
                Console.CursorVisible = true;
            }

            Console.WriteLine("Configuraciones aplicadas exitosamente.");
        }


        public void Ejecutar()
        {
            Console.Clear();
            Console.WriteLine("Has seleccionado la Opción 2.");
            // Aquí va la lógica de la página o módulo.
            Console.ReadKey();
        }
    }

    internal class AppSettings
    {
        public ApplicationSettings ApplicationSettings { get; set; }
    }

    public class ApplicationSettings
    {
        public string GUID { get; set; }
        public string GUIDD { get; set; }

        public DefaultConfigurations DefaultConfigurations { get; set; }
    }
    public class DefaultConfigurations
    {
        public string Language { get; set; }
        public string ThemeColor { get; set; }
        public string TextColor { get; set; }
        public string WindowSize { get; set; }
        public string CursorVisibility { get; set; }
        public string Title { get; set; }
    }
}
