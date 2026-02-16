using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

// Ollama server configuration
const string ollamaUrl = "http://localhost:11434/api/embed";
const string model = "bge-m3";

using var httpClient = new HttpClient();

Console.WriteLine("=== Ollama Text Embedding Demo ===");
Console.WriteLine($"Model: {model}");
Console.WriteLine("Server: http://localhost:11434/");
Console.WriteLine(new string('-', 40));

while (true)
{
    Console.WriteLine("\nEnter text to embed (or 'exit' to quit):");
    Console.Write("> ");
    
    var input = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Please enter some text.");
        continue;
    }
    
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye!");
        break;
    }
    
    try
    {
        Console.WriteLine("\nGenerating embedding...");
        
        // Create embedding request
        var request = new EmbeddingRequest
        {
            Model = model,
            Input = input
        };
        
        // Send request to Ollama server
        var response = await httpClient.PostAsJsonAsync(ollamaUrl, request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
        
        if (result?.Embeddings != null && result.Embeddings.Count > 0)
        {
            var embedding = result.Embeddings[0];
            
            Console.WriteLine($"\n--- Embedding Result ---");
            Console.WriteLine($"Vector Dimensions: {embedding.Count}");
            Console.WriteLine($"\nVector Values (first 20 dimensions):");
            
            var displayCount = Math.Min(20, embedding.Count);
            for (int i = 0; i < displayCount; i++)
            {
                Console.WriteLine($"  [{i}]: {embedding[i]:F8}");
            }
            
            if (embedding.Count > 20)
            {
                Console.WriteLine($"  ... ({embedding.Count - 20} more dimensions)");
            }
            
            Console.WriteLine($"\nFull Vector:");
            Console.WriteLine($"[{string.Join(", ", embedding.Select(v => v.ToString("F6")))}]");
        }
        else
        {
            Console.WriteLine("No embedding returned from the server.");
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\nConnection Error: {ex.Message}");
        Console.WriteLine("Make sure Ollama server is running at http://localhost:11434/");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError: {ex.Message}");
        Console.WriteLine("Make sure the bge-m3 model is available (run: ollama pull bge-m3)");
    }
}

// Request model for Ollama embedding API
public class EmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;
}

// Response model for Ollama embedding API
public class EmbeddingResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    
    [JsonPropertyName("embeddings")]
    public List<List<double>>? Embeddings { get; set; }
}
