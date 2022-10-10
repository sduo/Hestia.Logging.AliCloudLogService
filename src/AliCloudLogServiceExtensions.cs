using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.Collections.Generic;

namespace Hestia.Logging.AliCloudLogService
{
    public static class AliCloudLogServiceExtensions
    {
        public static ILoggingBuilder AddAliCloudLogService(this ILoggingBuilder builder,Action<Dictionary<string,string>,Dictionary<string,string>> with = null)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AliCloudLogServiceLoggerProvider>((services) => new AliCloudLogServiceLoggerProvider(services, with)));
            return builder;
        }
    }
}
