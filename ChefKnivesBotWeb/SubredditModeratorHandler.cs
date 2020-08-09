using ChefKnivesBotLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChefKnivesBotWeb
{
    public class SubredditModeratorHandler : AuthorizationHandler<SubredditModeratorPolicy>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SubredditModeratorPolicy requirement)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            if (Program.ChefKnivesService.IsSubredditModerator(context.User.Identity.Name))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
