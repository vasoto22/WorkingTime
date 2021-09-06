using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.IO;
using WorkingTime.Common.Models;
using WorkingTime.Functions.Entities;

namespace WorkingTime.Test.Helpers
{
    public class TestFactory
    {
        public static WorkingEntity GetWorkingEntity()
        {
            return new WorkingEntity
            {
                ETag = "*",
                PartitionKey = "WORKINGTIME",
                RowKey = Guid.NewGuid().ToString(),
                IdEmployee = 1,
                RegisterTime = DateTime.UtcNow,
                Type = 0,
                Consolidated = false
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid IdEmployee, WorkingTable workingRequest)
        {
            string request = JsonConvert.SerializeObject(workingRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{IdEmployee}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid IdEmployee)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{IdEmployee}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(WorkingTable workingRequest)
        {
            string request = JsonConvert.SerializeObject(workingRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static WorkingTable getWorkingRequest()
        {
            return new WorkingTable
            {
                IdEmployee = 1,
                Type = 0,
            };
        }
        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }
            return logger;
        }
    }
}
