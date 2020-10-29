using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response.General
{
    public class Response
    {
        public Response()
        {
        }

        public Response(string message, int success, object data)
        {
            Message = message;
            Success = success;
            Data = data;
        }

        public string Message { get; set; }

        public int Success { get; set; }

        public object Data { get; set; }

        public Response GetResponse(string message, int success, object data)
        {
            Response response = new Response(message, success, data);
            return response;
        }
    }
}
