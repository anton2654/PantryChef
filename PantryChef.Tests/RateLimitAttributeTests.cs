using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using PantryChef.Web.Filters;
using Xunit;

namespace PantryChef.Tests
{
    public class RateLimitAttributeTests
    {
        [Fact]
        public async Task Allows_UnderLimit()
        {
            var attr = new RateLimitAttribute(2);
            var http = new DefaultHttpContext();
            http.Connection.RemoteIpAddress = IPAddress.Parse("1.2.3.4");

            var actionDesc = new ActionDescriptor { DisplayName = "TestAction" };
            var actionCtx = new ActionContext(http, new RouteData(), actionDesc);
            var filters = new List<IFilterMetadata>();
            var context = new ActionExecutingContext(actionCtx, filters, new Dictionary<string, object>(), new object());

            ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(actionCtx, filters, new object()));

            await attr.OnActionExecutionAsync(context, next);
            Assert.Null(context.Result);

            // second request still allowed
            var context2 = new ActionExecutingContext(actionCtx, filters, new Dictionary<string, object>(), new object());
            await attr.OnActionExecutionAsync(context2, next);
            Assert.Null(context2.Result);
        }

        [Fact]
        public async Task Redirects_When_Exceeded()
        {
            var attr = new RateLimitAttribute(2);
            var http = new DefaultHttpContext();
            http.Connection.RemoteIpAddress = IPAddress.Parse("5.6.7.8");

            var actionDesc = new ActionDescriptor { DisplayName = "TestActionExceeded" };
            var actionCtx = new ActionContext(http, new RouteData(), actionDesc);
            var filters = new List<IFilterMetadata>();
            var next = new ActionExecutionDelegate(() => Task.FromResult(new ActionExecutedContext(actionCtx, filters, new object())));

            // two allowed
            var ctx1 = new ActionExecutingContext(actionCtx, filters, new Dictionary<string, object>(), new object());
            await attr.OnActionExecutionAsync(ctx1, next);
            Assert.Null(ctx1.Result);

            var ctx2 = new ActionExecutingContext(actionCtx, filters, new Dictionary<string, object>(), new object());
            await attr.OnActionExecutionAsync(ctx2, next);
            Assert.Null(ctx2.Result);

            // third should be redirected
            var ctx3 = new ActionExecutingContext(actionCtx, filters, new Dictionary<string, object>(), new object());
            await attr.OnActionExecutionAsync(ctx3, next);
            Assert.NotNull(ctx3.Result);
            Assert.IsType<RedirectResult>(ctx3.Result);
            var rr = (RedirectResult)ctx3.Result;
            Assert.Equal("/Home/Error", rr.Url);
        }
    }
}
