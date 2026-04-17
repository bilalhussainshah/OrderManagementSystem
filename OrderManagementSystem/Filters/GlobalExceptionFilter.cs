using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Serilog;
using OrderManagementSystem.Logger;

namespace OrderManagementSystem.Filters
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            Log.Error(context.Exception, "Unhandled API Exception");

            context.Response = context.Request.CreateResponse(
                HttpStatusCode.InternalServerError,
                new
                {
                    message = "An unexpected error occurred",
                    detail = context.Exception.Message
                });
        }
    }
}