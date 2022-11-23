using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NewsAggregatorAspNetCore.Filters;

public class InternetExplorerBlockerFilter : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();
        if (Regex.IsMatch(userAgent, "MSIE|Trident"))
        {
            context.Result = new ContentResult()
            {
                Content = "Please switch from IE to actual browser"
            };
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}