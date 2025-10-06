using Codeblaze.SemanticKernel.Connectors.Ollama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net;

// Configure logging
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

var logger = loggerFactory.CreateLogger<Program>();

// Configuration constants
const string ModelName = "deepseek-r1:1.5b";
const string OllamaUrl = "http://localhost:11434";
const int MaxRetries = 3;
const int TimeoutSeconds = 30;

try
{
    // Build kernel with proper HTTP client configuration
    var kernelBuilder = Kernel.CreateBuilder();

    // Add HTTP client services
    kernelBuilder.Services.AddHttpClient();

    kernelBuilder.AddOllamaChatCompletion(ModelName, OllamaUrl);

    var kernel = kernelBuilder.Build();

    // Validate connection before starting chat loop
    Console.WriteLine("🔄 Initializing connection to DeepSeek...");

    if (!await ValidateConnectionAsync(kernel, logger))
    {
        Console.WriteLine("❌ Failed to connect to DeepSeek. Please check your setup and try again.");
        return 1;
    }

    Console.WriteLine("✅ Connected to DeepSeek successfully!");
    Console.WriteLine("💡 Type 'exit', 'quit', or press Ctrl+C to end the conversation.\n");

    // Main chat loop with proper error handling
    await RunChatLoopAsync(kernel, logger);

    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Critical error occurred during application startup");
    Console.WriteLine($"❌ Critical error: {ex.Message}");
    Console.WriteLine("Please check your configuration and try again.");
    return 1;
}

static async Task<bool> ValidateConnectionAsync(Kernel kernel, ILogger logger)
{
    try
    {
        var testResponse = await kernel.InvokePromptAsync("Hello");
        return !string.IsNullOrEmpty(testResponse.ToString());
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "HTTP connection error during validation");
        Console.WriteLine("❌ Connection error: Unable to reach Ollama server.");
        Console.WriteLine("   Please ensure Ollama is running on http://localhost:11434");
        return false;
    }
    catch (TaskCanceledException)
    {
        logger.LogError("Connection timeout during validation");
        Console.WriteLine("❌ Connection timeout: Ollama server is not responding.");
        return false;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error during connection validation");
        Console.WriteLine($"❌ Validation error: {ex.Message}");
        return false;
    }
}

static async Task RunChatLoopAsync(Kernel kernel, ILogger logger)
{
    var cancellationTokenSource = new CancellationTokenSource();

    // Handle Ctrl+C gracefully
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cancellationTokenSource.Cancel();
        Console.WriteLine("\n👋 Goodbye!");
    };

    while (!cancellationTokenSource.Token.IsCancellationRequested)
    {
        try
        {
            Console.Write("You: ");
            var input = Console.ReadLine()?.Trim();

            // Handle exit commands
            if (string.IsNullOrEmpty(input) ||
                input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("👋 Goodbye!");
                break;
            }

            // Show typing indicator
            Console.Write("🤖 DeepSeek is thinking");
            var typingTask = ShowTypingIndicatorAsync(cancellationTokenSource.Token);

            // Get response with retry logic
            var response = await GetResponseWithRetryAsync(kernel, input, logger, cancellationTokenSource.Token);

            // Stop typing indicator
            cancellationTokenSource.Cancel();
            await typingTask;

            // Create new cancellation token for next iteration
            cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine($"\r🤖 DeepSeek: {response}\n");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n👋 Operation cancelled. Goodbye!");
            break;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in chat loop");
            Console.WriteLine($"\n❌ An error occurred: {ex.Message}");
            Console.WriteLine("Please try again or type 'exit' to quit.\n");
        }
    }
}

static async Task<string> GetResponseWithRetryAsync(Kernel kernel, string input, ILogger logger, CancellationToken cancellationToken)
{
    for (int attempt = 1; attempt <= MaxRetries; attempt++)
    {
        try
        {
            var response = await kernel.InvokePromptAsync(input);
            var result = response.ToString();

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException("Empty response received from model");
            }

            return result;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("ServiceUnavailable") || ex.Message.Contains("503"))
        {
            logger.LogWarning("Service unavailable on attempt {Attempt}", attempt);
            if (attempt == MaxRetries)
            {
                return "❌ Service is currently unavailable. Please try again later.";
            }
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("Operation was cancelled by user");
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Request timeout on attempt {Attempt}", attempt);
            if (attempt == MaxRetries)
            {
                return "❌ Request timed out. The model might be processing a complex query. Please try a simpler question.";
            }
            await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on attempt {Attempt}", attempt);
            if (attempt == MaxRetries)
            {
                return $"❌ Sorry, I encountered an error: {ex.Message}";
            }
            await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
        }
    }

    return "❌ Failed to get response after multiple attempts.";
}

static async Task ShowTypingIndicatorAsync(CancellationToken cancellationToken)
{
    var dots = "";
    try
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(500, cancellationToken);
            dots = dots.Length >= 3 ? "" : dots + ".";
            Console.Write($"\r🤖 DeepSeek is thinking{dots}   ");
        }
    }
    catch (OperationCanceledException)
    {
        // Expected when operation completes
    }
}