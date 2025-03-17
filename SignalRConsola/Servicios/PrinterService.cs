using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text.Json;

public class PrinterService
{
    public string GetInstalledPrinters()
    {
        List<string> printerList = new List<string>();
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            printerList.Add(printer);
        }
        
        string json = JsonSerializer.Serialize(printerList);
        return json;
    }
}