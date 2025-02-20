using System;
using System.Text.Json.Serialization;

/*
File: BatchProcessor\Core\Config\Models\ScriptType.cs
Summary: Defines supported script types for Rhino batch processing.
*/

namespace BatchProcessor.Core.Config.Models
{
    /// <summary>
    /// Supported script types.
    /// </summary>
    public enum ScriptType
    {
        /// <summary>
        /// Python script (.py)
        /// </summary>
        Python,
        /// <summary>
        /// Grasshopper script (.gh)
        /// </summary>
        Grasshopper,
        /// <summary>
        /// Grasshopper XML script (.ghx)
        /// </summary>
        GrasshopperXml
    }
}
