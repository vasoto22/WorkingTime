using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                IdEmployee = 123,
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
        private static Stream GenerateStreamFromString(string request)
        {
            throw new NotImplementedException();
        }
    }
}
