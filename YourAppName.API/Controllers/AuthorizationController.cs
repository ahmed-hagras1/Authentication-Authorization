using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YourAppName.Api.Base;
using YourAppName.Core.AppMetaData;
using YourAppName.Core.Features.Authorization.Commands.Models;
using YourAppName.Core.Features.Authorization.Queries.Models;
using YourAppName.Shared.Security;

namespace YourAppName.API.Controllers
{
    [ApiController]
    // Protects ALL endpoints in this controller so only users with the "Admin" role can access them
    //[Authorize(Roles = "Admin")]
    public class AuthorizationController : AppControllerBase
    {
        [Authorize(Policy = Permissions.Roles.View)]
        [HttpGet(Router.AuthorizationRouting.GetRolesList)]
        public async Task<IActionResult> GetRoleList(CancellationToken cancellationToken)
        {
            // Send the empty query to MediatR
            var response = await Mediator.Send(new GetRoleListQuery(), cancellationToken);

            // Return the formatted result
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Roles.View)]
        [HttpGet(Router.AuthorizationRouting.GetRoleById)]
        public async Task<IActionResult> GetRoleById([FromRoute] string id, CancellationToken cancellationToken)
        {
            // The [FromRoute] attribute grabs the {id} directly from the URL path
            var response = await Mediator.Send(new GetRoleByIdQuery(id),cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Roles.Create)]
        [HttpPost(Router.AuthorizationRouting.CreateRole)]
        public async Task<IActionResult> CreateRole([FromBody] AddRoleCommand command, CancellationToken cancellationToken)
        {
            // The CancellationToken is passed to MediatR here
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Roles.Edit)]
        [HttpPut(Router.AuthorizationRouting.EditRole)]
        public async Task<IActionResult> EditRole([FromBody] EditRoleCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Roles.Delete)]
        [HttpDelete(Router.AuthorizationRouting.DeleteRole)]
        public async Task<IActionResult> DeleteRole([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(new DeleteRoleCommand(id), cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Users.ViewRoles)]
        [HttpGet(Router.AuthorizationRouting.ManageUserRoles)]
        public async Task<IActionResult> ManageUserRoles([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(new ManageUserRolesQuery(id), cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Users.EditRoles)]
        [HttpPost(Router.AuthorizationRouting.UpdateUserRoles)]
        public async Task<IActionResult> UpdateUserRoles([FromBody] UpdateUserRolesCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Users.ViewClaims)]
        [HttpGet(Router.AuthorizationRouting.ManageUserClaims)]
        public async Task<IActionResult> ManageUserClaims([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(new ManageUserClaimsQuery(id), cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Users.EditClaims)]
        [HttpPut(Router.AuthorizationRouting.UpdateUserClaims)]
        public async Task<IActionResult> UpdateUserClaims([FromBody] UpdateUserClaimsCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Roles.Edit)]
        [HttpGet(Router.AuthorizationRouting.ManageRoleClaims)]
        public async Task<IActionResult> ManageRoleClaims([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(new ManageRoleClaimsQuery(id), cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Roles.Edit)]
        [HttpPut(Router.AuthorizationRouting.UpdateRoleClaims)]
        public async Task<IActionResult> UpdateRoleClaims([FromBody] UpdateRoleClaimsCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [Authorize(Policy = Permissions.Users.ChangeStatus)]
        [HttpPut(Router.AuthorizationRouting.ChangeUserStatus)]
        public async Task<IActionResult> ChangeUserStatus([FromBody] ChangeUserStatusCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
    }
}
