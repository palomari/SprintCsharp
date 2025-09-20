using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ProjetoCasaApostas.Utils;

public static class JsonExporter
{
    public static void Exportar<T>(List<T> dados, string fileName)
    {
        try
        {
            string projectDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

            string utilsDir = Path.Combine(projectDir, "Utils");

            if (!Directory.Exists(utilsDir))
                Directory.CreateDirectory(utilsDir);

            string filePath = Path.Combine(utilsDir, fileName);

            string json = JsonSerializer.Serialize(dados, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);

            Console.WriteLine($"Arquivo JSON gerado em: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao gerar JSON: " + ex.Message);
        }
    }
}
