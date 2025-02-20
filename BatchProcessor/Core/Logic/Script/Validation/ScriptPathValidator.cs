// Path: Core/Logic/Script/Validation/ScriptPathValidator.cs
using System;
using System.IO;
using BatchProcessor.DI.Interfaces.Script;
using BatchProcessor.DI.Interfaces.AbsRhino;


public class ScriptPathValidator : IScriptPathValidator
{
    private readonly ICommLineOut _output;

    public ScriptPathValidator(ICommLineOut output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public bool ValidatePath(string scriptPath)
    {
        if (string.IsNullOrEmpty(scriptPath))
        {
            _output.ShowError("Script path cannot be empty");
            return false;
        }

        if (!File.Exists(scriptPath))
        {
            _output.ShowError($"Script not found: {scriptPath}");
            return false;
        }

        try
        {
            // Verify read permissions
            using (File.OpenRead(scriptPath)) { }
            return true;
        }
        catch (Exception ex)
        {
            _output.ShowError($"Cannot access script file: {ex.Message}");
            return false;
        }
    }
}