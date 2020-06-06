using System;
using System.Net;

namespace TestSingalR.Errors
{
    public class RestException : Exception
    {
        public RestException(HttpStatusCode code, object errors = null)
        {
            Code = code;
            Errors = errors;
        }

        public HttpStatusCode Code { get; private set; }
        public object Errors { get; private set; }
    }
}
