using DtcDesk.Core;
using DtcDesk.Core.Parsing;

Console.WriteLine("╔═══════════════════════════════════════╗");
Console.WriteLine("║   DtcDesk - Parser de Códigos DTC    ║");
Console.WriteLine("╚═══════════════════════════════════════╝\n");

// Ejecutar los ejemplos de demostración
ParserDemo.RunExamples();

Console.WriteLine("\n=== PRUEBA INTERACTIVA ===");
Console.WriteLine("Pega códigos DTC (o presiona Enter para salir):\n");

var parser = new DtcParser();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        break;

    var codes = parser.Parse(input);
    
    if (codes.Count == 0)
    {
        Console.WriteLine("  ⚠️  No se encontraron códigos válidos\n");
        continue;
    }

    Console.WriteLine($"\n  ✓ Encontrados {codes.Count} código(s):\n");
    for (int i = 0; i < codes.Count; i++)
    {
        var category = parser.GetCodeCategory(codes[i]);
        Console.WriteLine($"    {i + 1}. {codes[i],-10} [{category}]");
    }
    Console.WriteLine();
}

Console.WriteLine("\n¡Hasta luego!");
