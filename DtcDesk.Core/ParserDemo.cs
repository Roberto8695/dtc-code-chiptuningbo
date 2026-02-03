using DtcDesk.Core.Parsing;

namespace DtcDesk.Core;

/// <summary>
/// Clase de prueba para demostrar el funcionamiento del DtcParser
/// </summary>
public class ParserDemo
{
    public static void RunExamples()
    {
        var parser = new DtcParser();

        Console.WriteLine("=== DEMO: DtcParser ===\n");

        // Ejemplo 1: Tu caso específico (horizontal a vertical)
        Console.WriteLine("Ejemplo 1: Códigos horizontales ordenados");
        Console.WriteLine("Entrada: \"C073 P0420 B1234 U0100 P0300 C073\"");
        var input1 = "C073 P0420 B1234 U0100 P0300 C073";
        var result1 = parser.Parse(input1);
        Console.WriteLine("Salida (vertical, sin duplicados):");
        for (int i = 0; i < result1.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {result1[i]}");
        }
        Console.WriteLine();

        // Ejemplo 2: Códigos hexadecimales
        Console.WriteLine("Ejemplo 2: Códigos hexadecimales");
        Console.WriteLine("Entrada: \"0001 0002 FFFF 1A2B 0001\"");
        var input2 = "0001 0002 FFFF 1A2B 0001";
        var result2 = parser.Parse(input2);
        Console.WriteLine("Salida:");
        for (int i = 0; i < result2.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {result2[i]}");
        }
        Console.WriteLine();

        // Ejemplo 3: Texto con ruido (simulando WinOLS)
        Console.WriteLine("Ejemplo 3: Texto con ruido");
        Console.WriteLine("Entrada: \"Error codes: P0420, P0300, C073 - B1234;U0100\"");
        var input3 = "Error codes: P0420, P0300, C073 - B1234;U0100";
        var result3 = parser.Parse(input3);
        Console.WriteLine("Salida:");
        for (int i = 0; i < result3.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {result3[i]}");
        }
        Console.WriteLine();

        // Ejemplo 4: Multilínea
        Console.WriteLine("Ejemplo 4: Texto multilínea");
        var input4 = @"P0420 P0300
C073 B1234
U0100";
        Console.WriteLine($"Entrada:\n{input4}");
        var result4 = parser.Parse(input4);
        Console.WriteLine("Salida (orden: izquierda a derecha, arriba a abajo):");
        for (int i = 0; i < result4.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {result4[i]}");
        }
        Console.WriteLine();

        // Ejemplo 5: Normalización (minúsculas a mayúsculas)
        Console.WriteLine("Ejemplo 5: Normalización");
        Console.WriteLine("Entrada: \"p0420 c073 b1234\"");
        var input5 = "p0420 c073 b1234";
        var result5 = parser.Parse(input5);
        Console.WriteLine("Salida (normalizadas a mayúsculas):");
        for (int i = 0; i < result5.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {result5[i]}");
        }
        Console.WriteLine();

        // Ejemplo 6: Estadísticas
        Console.WriteLine("Ejemplo 6: Estadísticas de parsing");
        var input6 = "P0420 P0300 C073 B1234 U0100 P0420 FFFF";
        Console.WriteLine($"Entrada: \"{input6}\"");
        var stats = parser.GetStats(input6);
        Console.WriteLine($"Estadísticas: {stats}");
        Console.WriteLine();

        // Ejemplo 7: Parse detallado
        Console.WriteLine("Ejemplo 7: Parse con información detallada");
        var input7 = "P0420 C073 B1234";
        var detailed = parser.ParseDetailed(input7);
        Console.WriteLine($"Entrada: \"{input7}\"");
        Console.WriteLine("Códigos con metadata:");
        foreach (var code in detailed)
        {
            Console.WriteLine($"  Pos {code.Position}: {code.Code} (Categoría: {code.Category}, Original: '{code.OriginalText}')");
        }
    }
}
