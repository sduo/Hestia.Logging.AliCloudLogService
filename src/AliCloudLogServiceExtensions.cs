using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Hestia.Logging.AliCloudLogService
{
    public static class AliCloudLogServiceExtensions
    {
        public static ILoggingBuilder AddAliCloudLogService(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AliCloudLogServiceLoggerProvider>((services) => new AliCloudLogServiceLoggerProvider(services)));
            return builder;
        }
    }
}
