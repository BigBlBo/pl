using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
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
                context.HttpContext.Response.Redirect("/Home/Index");
            }
        }
    }
}