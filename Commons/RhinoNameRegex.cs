using System.Text.RegularExpressions;

namespace BatchProcessor.Commons
{
    /// <summary>
    /// -----------------------------------------------------------------------------
    /// MODULE SUMMARY:
    /// This static class provides a single, centralized regex pattern for matching
    /// Rhino file names used throughout the batch processing code. The goal is to
    /// avoid duplicating the same pattern logic in multiple modules. 
    /// 
    /// Usage:
    ///   - Classes like RhinoFileNameValidator, BatchNameParser, and BatchNameValidator
    ///     can reference RhinoNameRegex.RhinoFileNameRegex instead of redefining the same 
    ///     pattern. This helps keep the naming rules consistent and maintainable.
    /// 
    /// By default, the pattern matches the format:
    ///   ^(\d{6}[LR])(\d*)-(.+)-([SR]\d{5})\.3dm$
    /// which indicates:
    ///   1) 6 digits followed by 'L' or 'R'
    ///   2) Optional digits
    ///   3) A keyword or descriptor
    ///   4) A dash, then 'S' or 'R' followed by 5 digits
    ///   5) A .3dm extension
    /// 
    /// Example valid file name: 300000L-foobar-S12345.3dm
    /// -----------------------------------------------------------------------------
    /// </summary>
    public static class RhinoNameRegex
    {
        /// <summary>
        /// This regex enforces the naming pattern for .3dm files in the batch pipeline.
        /// 
        /// Pattern Explanation:
        ///  - ^(\d{6}[LR]): Start with 6 digits plus 'L' or 'R' 
        ///    Example: 300000L or 654321R
        ///  - (\d*): Zero or more digits (optional part of the base PID)
        ///  - (.+): After the first dash, capture any keyword or descriptor in the file name
        ///  - ([SR]\d{5}): The second dash is followed by 'S' or 'R' plus 5 digits (e.g., S12345)
        ///  - \.3dm$: Finally, ensure the file extension is .3dm (case-insensitive)
        /// 
        /// RegexOptions:
        ///  - Compiled for performance
        ///  - IgnoreCase so we match .3dm in any letter casing
        /// </summary>
        public static readonly Regex RhinoFilePattern = new Regex(
            @"^(\d{6}[LR])(\d*)-(.+)-([SR]\d{5})\.3dm$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
    }
}
