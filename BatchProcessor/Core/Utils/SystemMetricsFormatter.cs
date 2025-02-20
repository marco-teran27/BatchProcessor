// Path: Core/Utils/SystemMetricsFormatter.cs
using System;

namespace BatchProcessor.Core.Utils
{
    /// <summary>
    /// Formats system metrics into human-readable strings.
    /// </summary>
    public static class SystemMetricsFormatter
    {
        public static string FormatPercentage(double value)
        {
            return $"{value:F2}%";
        }

        public static string FormatMemory(double megabytes)
        {
            if (megabytes >= 1024)
            {
                return $"{megabytes / 1024:F2} GB";
            }
            return $"{megabytes:F2} MB";
        }

        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            }
            return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        public static string FormatMetric(string name, double value, string unit)
        {
            return $"{name}: {value:F2} {unit}";
        }

        /// <summary>
        /// Formats values in bytes to human-readable sizes (KB, MB, GB, etc.)
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return $"{size:F2} {sizes[order]}";
        }

        /// <summary>
        /// Formats speed values (bytes per second) to human-readable format
        /// </summary>
        public static string FormatSpeed(double bytesPerSecond)
        {
            string[] sizes = { "B/s", "KB/s", "MB/s", "GB/s" };
            int order = 0;
            double speed = bytesPerSecond;

            while (speed >= 1024 && order < sizes.Length - 1)
            {
                order++;
                speed = speed / 1024;
            }

            return $"{speed:F2} {sizes[order]}";
        }

        /// <summary>
        /// Formats a count with appropriate scale (K for thousands, M for millions)
        /// </summary>
        public static string FormatCount(long count)
        {
            if (count >= 1_000_000)
                return $"{count / 1_000_000.0:F2}M";
            if (count >= 1_000)
                return $"{count / 1_000.0:F1}K";
            return count.ToString();
        }

        /// <summary>
        /// Formats a timestamp in the standard format for logs
        /// </summary>
        public static string FormatTimestamp(DateTime timestamp)
        {
            return timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// Formats time remaining with appropriate units
        /// </summary>
        public static string FormatTimeRemaining(TimeSpan remaining)
        {
            if (remaining.TotalDays >= 1)
                return $"{remaining.TotalDays:F1} days";
            if (remaining.TotalHours >= 1)
                return $"{remaining.TotalHours:F1} hours";
            if (remaining.TotalMinutes >= 1)
                return $"{remaining.TotalMinutes:F1} minutes";
            return $"{remaining.TotalSeconds:F0} seconds";
        }
    }
}