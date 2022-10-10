using Aliyun.Api.LogService;
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Infrastructure.Protocol.Http;
using Google.Protobuf;
using Hestia.Core;
using Hestia.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hestia.Logging.AliCloudLogService
{
    public class AliCloudLogServiceLoggerProvider : BatchingLoggerProvider
    {
        private ILogServiceClient client;

        public AliCloudLogServiceLoggerProvider(IServiceProvider services) : base(services)
        {
            string endpoint = configuration.GetValue<string>("endpoint", null);            
            string project = configuration.GetValue<string>("project", null);
            string ak = configuration.GetValue<string>("ak", null);
            string sk = configuration.GetValue<string>("sk", null);
            client = LogServiceClientBuilders.HttpBuilder
                .Endpoint(endpoint, project)
                .Credential(ak, sk)
                .Build();
        }

        public override string Name => "AliCloudLogService";

        protected override async Task WriteMessagesAsync(IEnumerable<Log> logs, CancellationToken token)
        {
            string store = configuration.GetValue<string>("store", null);

            foreach (var log in logs)
            {
                var tags = new Dictionary<string, string>()
                {
                    { "OS", $"{Environment.OSVersion}" },
                    { "User",Environment.UserName },
                    { "UI", $"{Environment.UserInteractive}" },
                    { "Module",Path.GetFileName(Environment.CommandLine ?? Environment.ProcessPath) },                    
                    { "Category",log.Category },                    
                };

                var contents = new Dictionary<string, string>() {
                    { "Id",string.Format(configuration.GetValue("Id","{0}"),log.Id) },
                    { "OS", $"{Environment.OSVersion}" },
                    { "User",Environment.UserName },
                    { "UI", $"{Environment.UserInteractive}" },
                    { "ProcessId", $"{Environment.ProcessId}" },                    
                    { "Path",Environment.ProcessPath },
                    { "CommandLine",Environment.CommandLine },
                    { "Timestamp",$"{log.Timestamp}" },
                    { "Category",log.Category },
                    { "Level", $"{log.Level}" },
                    { "EventId", $"{log.Event.Id}" },
                    { "EventName", log.Event.Name },
                    { "Message", log.Message },
                    { "Scopes", Utility.ToJson(log.Scopes) }
                };

                if(log.Exception is not null)
                {
                    var exception = new StringBuilder(log.Exception.Message);
                    if(log.Exception.InnerException is not null)
                    {
                        exception.Append($"({log.Exception.GetBaseException().Message})");
                    }
                    contents.Add("Exception", exception.ToString());
                    if (configuration.GetValue($"Stack", true) && !string.IsNullOrEmpty(log.Exception.StackTrace))
                    {
                        contents.Add("Stack",log.Exception.StackTrace);
                    }
                }

                var response = await client.PostLogStoreLogsAsync(store, new LogGroupInfo
                {
                    Topic = Path.GetFileName(Environment.ProcessPath),
                    Source = Environment.MachineName,
                    LogTags = tags,
                    Logs = new List<LogInfo>
                    {
                        new LogInfo
                        {
                            Time = log.Timestamp,
                            Contents = contents
                        }
                    }
                });

                var trace = new StringBuilder($"{response.GetHttpStatusCode()}");                
                if (!response.IsSuccess)
                {
                    trace.Append($":{response.Error}");
                }
                Trace.WriteLine($"[{nameof(AliCloudLogServiceLoggerProvider)}]{store}:{trace}");
            }
        }
    }
}