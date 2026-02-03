using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class FixCsv
{
    static void Main()
    {
        string inputFile = @"c:\Users\rober\Desktop\codigos_dtc\codigos_DTC.csv";
        string outputFile = @"c:\Users\rober\Desktop\codigos_dtc\codigos_DTC_fixed.csv";

        try
        {
            var lines = File.ReadAllLines(inputFile, Encoding.UTF8);
            
            using (var writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                // Escribir nuevo encabezado con columna Notes
                writer.WriteLine("Code,Description,Category,Source,Notes");
                
                int count = 0;
                // Procesar cada línea (saltar encabezado)
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    
                    // Limpiar espacios múltiples
                    string cleanedLine = Regex.Replace(lines[i], @"\s+", " ");
                    
                    // Añadir columna Notes vacía
                    writer.WriteLine(cleanedLine + ",");
                    count++;
                }
                
                Console.WriteLine($"✅ Archivo corregido creado: codigos_DTC_fixed.csv");
                Console.WriteLine($"Total de códigos procesados: {count}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
