// Path: Core/Logic/Script/Validation/ScriptCompletionValidator.cs
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.Script;
using BatchProcessor.DI.Interfaces.AbsRhino;
public class ScriptCompletionValidator : IScriptCompletionValidator
{
    private readonly string _outputDir;
    private readonly string _projectName;
    private readonly ICommLineOut _output;

    public ScriptCompletionValidator(
        string outputDir,
        string projectName,
        ICommLineOut output)
    {
        _outputDir = outputDir ?? throw new ArgumentNullException(nameof(outputDir));
        _projectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public async Task<(bool success, string details)> ValidateCompletion(
        string fileName, 
        string scriptPath)
    {
        var completionFilePath = GetCompletionFilePath(fileName);
        if (!File.Exists(completionFilePath))
        {
            return (false, "Completion file not found");
        }

        try
        {
            var content = await File.ReadAllTextAsync(completionFilePath);
            var completion = JsonSerializer.Deserialize<CompletionInfo>(content);
            return completion != null 
                ? (completion.Success, completion.Details ?? string.Empty)
                : (false, "Invalid completion file format");
        }
        catch (Exception ex)
        {
            return (false, $"Error reading completion file: {ex.Message}");
        }
    }

    public bool HasValidCompletionFiles(string fileName)
    {
        var path = GetCompletionFilePath(fileName);
        return File.Exists(path);
    }

    private string GetCompletionFilePath(string fileName)
    {
        return Path.Combine(_outputDir, $"{fileName}_{_projectName}_completion.json");
    }

    private class CompletionInfo
    {
        public bool Success { get; set; }
        public string? Details { get; set; }
    }
}