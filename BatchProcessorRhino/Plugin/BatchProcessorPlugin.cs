using System;
using Rhino;
using Rhino.PlugIns;
using Microsoft.Extensions.DependencyInjection;
using BatchProcessor.DI;
using BatchProcessor.DI.Interfaces.AbsRhino;
using BatchProcessor.DI.Interfaces.Batch;
using BatchProcessor.DI.Interfaces.Config;
using BatchProcessor.DI.Interfaces.Logging;
using BatchProcessor.DI.Interfaces.Monitoring;
using BatchProcessor.DI.Interfaces.Services;
using BatchProcessor.DI.Interfaces.Script;
using BatchProcessor.Core.Config.Validation;
using BatchProcessor.Core.IO.Config;
using BatchProcessor.Core.IO.Logging;
using BatchProcessor.Core.IO.Script;
using BatchProcessor.Core.IO.Batch;
using BatchProcessor.Core.Logging.Batch;
using BatchProcessor.Core.Logic.Batch;
using BatchProcessor.Core.Logic.Validation;
using BatchProcessor.Core.Logic.Completion;
using BatchProcessor.Core.Logic.Error;
using BatchProcessor.Core.Logic.Script;
using BatchProcessor.Core.Logic.Script.Validation;
using BatchProcessor.Core.Logic.Timeout;
using BatchProcessor.Core.Metrics.Batch;
using BatchProcessor.Core.Metrics.System;
using BatchProcessor.Core.Services;
using BatchProcessorRhino.CommandLine;
using BatchProcessorRhino.Logic;
using BatchProcessorRhino.Services;
using BatchProcessorRhino.UI;
using BatchProcessor.Di.Interfaces.AbsRhino;
using BatchProcessor.DI.Interfaces.Error;

namespace BatchProcessorRhino.Plugin
{
    public class BatchProcessorPlugin : PlugIn
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public BatchProcessorPlugin()
        {
            Instance = this;
            ConfigureDependencyInjection();
        }

        public static BatchProcessorPlugin Instance { get; private set; } = null!;

        private void ConfigureDependencyInjection()
        {
            var services = new ServiceCollection();

            // Register Core Configuration and Validation (stateless; singleton)
            services.AddSingleton<IConfigParser, ConfigParser>();
            services.AddSingleton<IConfigSelUI, ConfigSelUI>();
            services.AddSingleton<IConfigValidator, ConfigValidator>();
            services.AddSingleton<IDirectoryValidator, DirectoryValidator>();
            services.AddSingleton<IPIDValidator, PIDValidator>();
            services.AddSingleton<IProjectNameValidator, ProjectNameValidator>();
            services.AddSingleton<IReprocessValidator, ReprocessValidator>();
            services.AddSingleton<IRhinoFileNameValidator, RhinoFileNameValidator>();
            services.AddSingleton<IScriptSettingsValidator, ScriptSettingsValidator>();
            services.AddSingleton<ITimeOutValidator, TimeOutValidator>();

            // Register Rhino-specific implementations (singleton)
            services.AddSingleton<IRhinoApp, RhinoAppWrapper>();
            services.AddSingleton<ICommLineOut, CommLineOutput>();
            services.AddSingleton<ICommInDelegation, CommInDelegation>();

            // Register Logging (singleton)
            services.AddSingleton<IBatchLogger, BatchLogger>();
            services.AddSingleton<ILogFormatter, LogFormatter>();
            services.AddSingleton<ILogWriter, LogFileWriter>();

            // Register Monitoring (transient)
            services.AddTransient<ISystemsMonitor, SystemMonitor>();

            // Register Resource Cleanup (transient)
            services.AddTransient<IResourceCleanUpService, ResourceCleanupService>();

            // Register Script-related (singleton)
            services.AddSingleton<IScriptCompletionValidator, ScriptCompletionValidator>();
            services.AddSingleton<IScriptExecutor, ScriptExecutor>();
            services.AddSingleton<IScriptParser, ScriptParser>();
            services.AddSingleton<IScriptPathValidator, ScriptPathValidator>();
            services.AddSingleton<IScriptScanner, ScriptScanner>();
            services.AddSingleton<IScriptTypeValidator, ScriptTypeValidator>();
            services.AddSingleton<IScriptValidator, ScriptValidationManager>();

            // Register Batch Processing (transient)
            services.AddTransient<IBatchCompletionManager, BatchCompletionManager>();
            services.AddTransient<IBatchNameValidator, BatchNameValidator>();
            services.AddTransient<IBatchService, BatchService>();

            // Register Logic/Batch Components (transient)
            services.AddTransient<BatchCancel>();
            services.AddTransient<BatchDirScanner>();
            services.AddTransient<BatchFileEnd>();
            services.AddTransient<BatchFileOpen>();
            services.AddTransient<BatchKill>();
            services.AddTransient<BatchReprocess>();
            services.AddTransient<BatchState>();

            // Register Logic/Completion (transient)
            services.AddTransient<TimeoutManager>();

            // Register Logic/Error (transient)
            services.AddTransient<IErrorManager, ErrorManager>();

            // Register Logic/Script (transient)
            services.AddTransient<ScriptRetryManager>();

            // Register Metrics (transient)
            services.AddTransient<BatchCollector>();
            services.AddTransient<BatchMonitor>();
            services.AddTransient<BatchMetricStorage>();
            services.AddTransient<SystemCollector>();
            services.AddTransient<SystemMonitor>();
            services.AddTransient<SystemStorage>();

            // Register Services (transient)
            services.AddTransient<BatchConfig>();
            services.AddTransient<BatchRetry>();
            services.AddTransient<BatchService>();
            services.AddTransient<TheOrchestrator>();

            // Build the service provider
            ServiceProvider = DIContainerConfig.ConfigureServices(services, useRhino: true);
        }
    }
}