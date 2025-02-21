
using BatchProcessor.Core.Config.Models;

public interface IBatchNameValidator
{
    bool IsValidFileName(string fileName, out BatchNameComponents components);
    bool MatchesConfigRules(
        BatchNameComponents components, 
        PIDSettings pidSettings,
        RhinoFileNameSettings fileNameSettings);
}