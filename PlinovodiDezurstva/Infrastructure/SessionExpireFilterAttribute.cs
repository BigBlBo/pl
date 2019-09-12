using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using PlinovodiDezurstva.Models;

namespace PlinovodiDezurstva.Infrastructure
{
    public class SessionExpireFilterAttribute : ActionFilterAttribute //, IAsyncActionFilter
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ISession session =  context.HttpContext.Session;

            // check if session is supported
            if (session == null || session.GetJson<SessionLogIn>("ses") == null)
            {
                context.Result = new RedirectToRouteResult(new
                           RouteValueDictionary(new { controller = "Home", action = "Index" }));
            }
        }
    }
}