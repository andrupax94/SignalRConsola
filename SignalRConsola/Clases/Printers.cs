using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Printing;
using IronPrint;
using IronPdf;
using System.Text;
public class Printers
{
    private readonly string outputFilePath;
    public Printers()
    {
        this.outputFilePath = Path.Combine(GetSolutionDirectory(), "impresion/output/archivo.pdf");
    }
    public Printers(string outputFilePath)
    {
        this.outputFilePath = outputFilePath;
    }

    public bool PrintToPDF(string content, string printerName)
    {
        if (!PrinterExists(printerName))
        {
            Console.WriteLine("La impresora especificada no existe.");
            return false;
        }


        switch (printerName)
        {
            case "Microsoft Print to PDF":
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.PrintPage += (sender, e) =>
                {
                    e.Graphics.DrawString(content, new Font("Arial", 12), Brushes.Black, e.MarginBounds);
                };

                printDocument.PrinterSettings.PrintFileName = outputFilePath;
                printDocument.PrinterSettings.PrintToFile = true;
                printDocument.PrinterSettings.Duplex = Duplex.Simplex;
                printDocument.PrintController = new StandardPrintController();
                printDocument.Print();
                return true;
                break;
            case "Foxit PDF Editor Printer":

                return IronPrintDefaultPDF(content, printerName);
                break;
            case "POS-80C (2 redireccionado)":
                return DriverPrintDefault(content, printerName);
                break;
            case "POS-58C (2 redireccionado)":
                return DriverPrintDefault(content, printerName);
                break;
            default:
                Console.WriteLine("No Hay configuraciones para esta impresora se usara la configuracion por default");
                return IronPrintDefaultPDF(content, printerName);

                break;
        }

    }

    private bool DriverPrintDefault(string content, string printerName)
    {
        try
        {
            // Bloque ASCII a imprimir
            string asciiAtecresa = @"                                                                                                                                                                                      
                  ███                                                      
                ███████                                                    
       ████████  ███    ███████  ██████ ███▓██░ ███████  ██████  ████████  
     ███   ███  ░███  ███   ███▒███     ███░   ██   ███ ███    ▒███   ███  
     ███   ███  ███   ███████  ███     ███    ███████    ░████ ███   ███   
    ███  ████   ███   ███      ███     ███    ███          ███ ███  ████   
    ████ ███   █████  ███████  █████ ███░     ▒████    █████   █████ ███   
";

            // Configurar el documento de impresión
            PrintDocument printDocument = new PrintDocument();

            // Asignar la impresora específica
            printDocument.PrinterSettings.PrinterName = printerName;

            // Ajustar el tamaño del papel (puedes ajustar según tu impresora de etiquetas)
            printDocument.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Etiqueta", 300, 1500); // Ejemplo: 380 x 600 unidades (centésimas de pulgada)

            // Manejar el evento PrintPage
            printDocument.PrintPage += (sender, e) =>
            {
                // Configurar la fuente y las dimensiones
                Font font = new Font("Courier New", 4); // Fuente monoespaciada ideal para ASCII
                float leftMargin = 10; // Margen izquierdo
                float topMargin = 10;  // Margen superior
                float maxWidth = 270;  // Ancho máximo
                float lineHeight = 10; // Altura de una línea de texto

                // Imprimir el bloque ASCII (ajustar líneas según ancho)
                string[] asciiLines = SplitTextToFitWidth(asciiAtecresa, font, e.Graphics, maxWidth);
                foreach (string line in asciiLines)
                {
                    e.Graphics.DrawString(line, font, Brushes.Black, leftMargin, topMargin);
                    topMargin += lineHeight;

                }

                // Espacio entre ASCII y texto adicional
                topMargin += 60;
                var fontContent = new Font("Courier New", 8);
                // Imprimir el contenido (content)
                string[] contentLines = SplitTextToFitWidth(content, fontContent, e.Graphics, maxWidth);
                foreach (string line in contentLines)
                {
                    e.Graphics.DrawString(line, fontContent, Brushes.Black, leftMargin, topMargin);
                    topMargin += lineHeight;
                }
                topMargin += 389;
                e.Graphics.DrawString("----------------------------", fontContent, Brushes.Black, leftMargin, topMargin);
            };

            // Iniciar la impresión
            printDocument.Print();
            Console.WriteLine("Impresión realizada con éxito.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la impresión: {ex.Message}");
            return false;
        }
    }

    private string[] SplitTextToFitWidth(string text, Font font, Graphics graphics, float maxWidth)
    {
        List<string> lines = new List<string>();
        string[] words = text.Split(' '); // Dividir el texto en palabras
        StringBuilder line = new StringBuilder();

        foreach (string word in words)
        {
            // Probar agregar la palabra a la línea actual
            string testLine = line.Length == 0 ? word : line + " " + word;
            SizeF textSize = graphics.MeasureString(testLine, font);

            // Si el ancho supera el máximo permitido, guarda la línea y empieza una nueva
            if (textSize.Width > maxWidth)
            {
                lines.Add(line.ToString());
                line.Clear();
                line.Append(word);
            }
            else
            {
                line.Append(line.Length == 0 ? word : " " + word);
            }
        }

        // Agregar la última línea
        if (line.Length > 0)
        {
            lines.Add(line.ToString());
        }

        return lines.ToArray();
    }


    private bool IronPrintDefault(string content, string printerName)
    {
        string documentContent = content;
        var renderer = new ChromePdfRenderer();
        var pdfFromHtmlString = renderer.RenderHtmlAsPdf(documentContent);
        pdfFromHtmlString.Print(printerName);
        return true;
    }
    private bool IronPrintDefaultPDF(string content, string printerName)
    {
        // Contenido del documento
        string documentContent = content;
        var renderer = new ChromePdfRenderer();
        var pdfFromHtmlString = renderer.RenderHtmlAsPdf(documentContent);
        pdfFromHtmlString.SaveAs(outputFilePath);

        // Configurar los ajustes de impresión
        PrintSettings printSettings = new PrintSettings
        {
            PrinterName = "Foxit PDF Editor Printer", // Especifica el nombre de la impresora
            Dpi = 300
        };

        try
        {
            // Imprimir el documento PDF
            Printer.Print(outputFilePath, printSettings);
            Console.WriteLine("Documento impreso correctamente.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al imprimir el documento: {ex.Message}");
            return false;
        }
        finally
        {
            // Eliminar el archivo PDF temporal (opcional)
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
        }
    }
    private bool PrinterExists(string printerName)
    {
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            if (printer.Equals(printerName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
    static string GetSolutionDirectory()
    {
        var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        for (int i = 0; i < 4; i++) // Navegar hacia arriba 3 niveles para llegar a la raíz de la solución
        {
            currentDirectory = Directory.GetParent(currentDirectory).FullName;
        }
        return currentDirectory;
    }

}
