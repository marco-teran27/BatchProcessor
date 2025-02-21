using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace BatchProcessor.Core.Metrics.System
{
    /// <summary>
    /// Collects and processes system metrics (CPU, Memory)
    /// </summary>
    public class SystemCollector
    {
        private readonly ILogger<SystemCollector> _logger;
        private readonly ConcurrentQueue<SystemMetric> _metricsQueue;
        private readonly int _maxQueueSize;
        private DateTime _collectionStartTime;
        private bool _isCollecting;

        public SystemCollector(ILogger<SystemCollector> logger, int maxQueueSize = 1000)
        {
            _logger = logger;
            _maxQueueSize = maxQueueSize;
            _metricsQueue = new ConcurrentQueue<SystemMetric>();
            _collectionStartTime = DateTime.Now;
        }

        /// <summary>
        /// Starts metrics collection
        /// </summary>
        public void StartCollection()
        {
            _isCollecting = true;
            _collectionStartTime = DateTime.Now;
            _logger.LogInformation("System metrics collection started");
        }

        /// <summary>
        /// Stops metrics collection
        /// </summary>
        public void StopCollection()
        {
            _isCollecting = false;
            _logger.LogInformation("System metrics collection stopped");
        }

        /// <summary>
        /// Adds a new metric reading to the collection
        /// </summary>
        public void AddMetric(double cpuPercentage, double memoryMB)
        {
            if (!_isCollecting) return;

            var metric = new SystemMetric
            {
                Timestamp = DateTime.Now,
                CpuPercentage = cpuPercentage,
                MemoryMB = memoryMB
            };

            _metricsQueue.Enqueue(metric);

            // Maintain queue size
            while (_metricsQueue.Count > _maxQueueSize)
            {
                _metricsQueue.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Gets current system metrics
        /// </summary>
        public SystemMetricSummary GetCurrentMetrics()
        {
            return new SystemMetricSummary
            {
                CollectionStartTime = _collectionStartTime,
                CurrentTime = DateTime.Now,
                MetricsCollected = _metricsQueue.Count,
                AverageCpuPercentage = CalculateAverageCpu(),
                AverageMemoryMB = CalculateAverageMemory(),
                PeakCpuPercentage = CalculatePeakCpu(),
                PeakMemoryMB = CalculatePeakMemory()
            };
        }

        /// <summary>
        /// Gets all collected metrics
        /// </summary>
        public IReadOnlyCollection<SystemMetric> GetMetricsHistory()
        {
            return _metricsQueue.ToArray();
        }

        private double CalculateAverageCpu()
        {
            if (_metricsQueue.IsEmpty) return 0;
            return _metricsQueue.Average(m => m.CpuPercentage);
        }

        private double CalculateAverageMemory()
        {
            if (_metricsQueue.IsEmpty) return 0;
            return _metricsQueue.Average(m => m.MemoryMB);
        }

        private double CalculatePeakCpu()
        {
            if (_metricsQueue.IsEmpty) return 0;
            return _metricsQueue.Max(m => m.CpuPercentage);
        }

        private double CalculatePeakMemory()
        {
            if (_metricsQueue.IsEmpty) return 0;
            return _metricsQueue.Max(m => m.MemoryMB);
        }
    }

    public class SystemMetric
    {
        public DateTime Timestamp { get; set; }
        public double CpuPercentage { get; set; }
        public double MemoryMB { get; set; }
    }

    public class SystemMetricSummary
    {
        public DateTime CollectionStartTime { get; set; }
        public DateTime CurrentTime { get; set; }
        public int MetricsCollected { get; set; }
        public double AverageCpuPercentage { get; set; }
        public double AverageMemoryMB { get; set; }
        public double PeakCpuPercentage { get; set; }
        public double PeakMemoryMB { get; set; }
    }
}