// Path: Core/Logic/Script/Validation/ScriptTypeValidator.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.DI.Interfaces.Script;

public class ScriptTypeValidator : IScriptTypeValidator
{
    private static readonly Dictionary<ScriptType, string> _validExtensions = new()
    {
        { ScriptType.Python, ".py" },
        { ScriptType.Grasshopper, ".gh" },
        { ScriptType.GrasshopperXml, ".ghx" }
    };

    public bool IsValidScriptType(string scriptPath, ScriptType expectedType)
    {
        if (string.IsNullOrEmpty(scriptPath)) return false;

        var extension = Path.GetExtension(scriptPath).ToLowerInvariant();
        return _validExtensions.TryGetValue(expectedType, out var expectedExt) && 
               extension == expectedExt;
    }

    public ScriptType? DetermineScriptType(string scriptPath)
    {
        if (string.IsNullOrEmpty(scriptPath)) return null;

        var extension = Path.GetExtension(scriptPath).ToLowerInvariant();
        return _validExtensions
            .FirstOrDefault(x => x.Value == extension)
            .Key;
    }
}