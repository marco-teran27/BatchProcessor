using System;

public class BatchNameComponents
{
    /// <summary>
    /// Base PID (6 digits + L/R), e.g., "300000L"
    /// </summary>
    public string BasePid { get; set; } = string.Empty;

    /// <summary>
    /// Optional digits after PID, e.g., "123"
    /// </summary>
    public string OptionalDigits { get; set; } = string.Empty;

    /// <summary>
    /// Keyword between hyphens, e.g., "damold"
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// SR number (S/R + 5 digits), e.g., "S12345"
    /// </summary>
    public string SrNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full PID including SR number
    /// </summary>
    public string FullPid => $"{BasePid}-{SrNumber}";

    /// <summary>
    /// Gets the complete file name without extension
    /// </summary>
    public string GetFileName()
    {
        var optionalPart = string.IsNullOrEmpty(OptionalDigits) ? "" : OptionalDigits;
        return $"{BasePid}{optionalPart}-{Keyword}-{SrNumber}";
    }

    /// <summary>
    /// Gets the complete file name with .3dm extension
    /// </summary>
    public string GetFileNameWithExtension() => $"{GetFileName()}.3dm";
}