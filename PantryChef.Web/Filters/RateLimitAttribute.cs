using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PantryChef.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requests = new();
        private static readonly ConcurrentDictionary<string, object> _locks = new();

        public int MaxRequestsPerMinute { get; }

        public RateLimitAttribute(int maxRequestsPerMinute)
        {
            MaxRequestsPerMinute = maxRequestsPerMinute;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (http.Request.Headers.TryGetValue("X-Forwarded-For", out var xff))
            {
                var first = xff.ToString().Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(first)) ip = first;
            }

            var key = $"{context.ActionDescriptor.DisplayName}:{ip}";
            var q = _requests.GetOrAdd(key, _ => new Queue<DateTime>());
            var lk = _locks.GetOrAdd(key, _ => new object());
            var now = DateTime.UtcNow;

            lock (lk)
            {
                while (q.Count > 0 && (now - q.Peek()).TotalSeconds >= 60) q.Dequeue();
                if (q.Count >= MaxRequestsPerMinute)
                {
                    context.Result = new RedirectResult("/Home/Error");
                    return;
                }
                q.Enqueue(now);
            }

            await next();
        }
    }
}
