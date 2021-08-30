using System;
using System.Collections.Generic;
using System.Text;

namespace WorkingTime.Common.Responses
{
    public class Response
    {
        public int IdEmployee { get; set; }
        public DateTime RegisteredTime { get; set; }
        public string Message { get; set; }

        public object Result { get; set; }
    }
}
