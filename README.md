# 🧠 DeepSeek .NET Console Application

A simple **.NET 9** console app that integrates with the **DeepSeek AI model** running on **Ollama**, powered by **Microsoft Semantic Kernel**.

---

## ✨ Features

- 🗨️ Interactive console chat interface with DeepSeek AI  
- ⚙️ Built with **.NET 9** and **C# 13**  
- 🧩 Uses **Microsoft Semantic Kernel** for AI integration  
- 🤖 Connects to an **Ollama-hosted DeepSeek** model  

---

## 🧰 Prerequisites

Before running the app, make sure you’ve got:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)  
- [Ollama](https://ollama.ai/) installed and running  
- The **DeepSeek** model available in Ollama  

---

## ⚙️ Setup Instructions

### 1️⃣ Install Ollama (if not already)
Follow the guide on [Ollama’s official site](https://ollama.ai/download).

### 2️⃣ Pull the DeepSeek model
```bash
ollama pull deepseek-r1:1.5b
```

### 3️⃣ Start the Ollama server
```bash
ollama serve
```
By default, the server runs at:  
**`http://localhost:11434`**

### 4️⃣ Clone and run this project
```bash
git clone <repo-url>
cd DeepseekDotnet
dotnet restore
dotnet run
```

---

## 💬 Usage

1. Start the app:
   ```bash
   dotnet run
   ```
2. Type your question or prompt in the console.  
3. The app sends it to the **DeepSeek model** and displays the response.  
4. Continue chatting, or press **Ctrl + C** to exit.  

---

## 📦 Dependencies

| Package | Version | Description |
|----------|----------|-------------|
| `Microsoft.SemanticKernel` | 1.65.0 | Core Semantic Kernel framework |
| `Codeblaze.SemanticKernel.Connectors.Ollama` | 1.3.1 | Ollama connector for Semantic Kernel |

---

## 🗂️ Project Structure

```
DeepseekDotnet/
├── Program.cs              # Main application entry point
├── DeepseekDotnet.csproj   # Project configuration
├── README.md               # This file
└── .gitignore              # Git ignore rules
```

---

## ⚙️ Configuration

The app connects to the following by default:
- **Model:** `deepseek-r1:1.5b`  
- **Ollama URL:** `http://localhost:11434`

To use a different model or endpoint, edit the parameters in **`Program.cs`**:
```csharp
.AddOllamaChatCompletion("model-name", "http://your-server-ip:11434");
```

---

## 🚑 Troubleshooting

| Issue | Fix |
|--------|------|
| ❌ **Connection refused** | Ensure Ollama is running on port 11434 |
| ⚠️ **Model not found** | Run `ollama pull deepseek-r1:1.5b` |
| 🧱 **Build errors** | Confirm that **.NET 9 SDK** is installed |

---

## 📜 License

This project is provided **as-is** for **educational and development purposes** only.  
Use it, tweak it, learn from it — just don’t forget to share the love ❤️
