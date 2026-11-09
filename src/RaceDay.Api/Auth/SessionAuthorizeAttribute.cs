using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Auth;

/// <summary>
/// Blocks the request unless a server-side session holds a user ID and role.
/// Pass a role to also reject the other RaceDay role with 403.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    public string? AllowedRole { get; }

    public SessionAuthorizeAttribute()
    {
    }

    public SessionAuthorizeAttribute(string allowedRole)
    {
        AllowedRole = allowedRole;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var session = context.HttpContext.Session;
        await session.LoadAsync();

        var userId = session.GetInt32(SessionKeys.UserId);
        var role = session.GetString(SessionKeys.Role);

        if (userId is null || string.IsNullOrWhiteSpace(role))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Please log in before calling this endpoint." });
            return;
        }

        if (!string.IsNullOrWhiteSpace(AllowedRole)
            && !string.Equals(role, AllowedRole, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new { message = $"This endpoint is for the {AllowedRole} role only." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}
