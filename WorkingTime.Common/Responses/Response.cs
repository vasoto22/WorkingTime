using System;

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
