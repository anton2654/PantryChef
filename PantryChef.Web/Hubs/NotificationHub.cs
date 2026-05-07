using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PantryChef.Web.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
