using MediatR;
using Microsoft.Extensions.Localization;
using YourAppName.Core.Bases;
using YourAppName.Core.Features.Authorization.Commands.Models;
using YourAppName.Service.Abstracts;
using YourAppName.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Commands.Handlers
{
    public class AuthorizationCommandHandler : ResponseHandler,
        IRequestHandler<AddRoleCommand, Response<string>>,
        IRequestHandler<EditRoleCommand, Response<string>>,
        IRequestHandler<DeleteRoleCommand, Response<string>>,
        IRequestHandler<UpdateUserRolesCommand, Response<string>>,
        IRequestHandler<UpdateRoleClaimsCommand, Response<string>>,
        IRequestHandler<UpdateUserClaimsCommand, Response<string>>,
        IRequestHandler<ChangeUserStatusCommand, Response<string>>
    {
        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<SharedResources> _localizer;
        #endregion

        #region Constructor
        public AuthorizationCommandHandler(IAuthorizationService authorizationService,
            IStringLocalizer<SharedResources> localizer) : base(localizer)
        {
            _authorizationService = authorizationService;
            _localizer = localizer;
        }
        #endregion

        // CancellationToken is accepted here and passed down naturally by MediatR
        public async Task<Response<string>> Handle(AddRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _authorizationService.AddRoleAsync(request.RoleName);

            if (result == "RoleIsExist")
            {
                return BadRequest<string>(_localizer[SharedResourcesKeys.RoleIsExist]);
            }
            else if (result == "Success")
            {
                return Success<string>(_localizer[SharedResourcesKeys.Created]);
            }

            return BadRequest<string>(_localizer[SharedResourcesKeys.BadRequest]);
        }
        public async Task<Response<string>> Handle(EditRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _authorizationService.EditRoleAsync(request.Id, request.Name);

            if (result == "NotFound")
            {
                return NotFound<string>(_localizer[SharedResourcesKeys.NotFound]);
            }
            else if (result == "Success")
            {
                return Success<string>(_localizer[SharedResourcesKeys.Updated]);
            }

            return BadRequest<string>(_localizer[SharedResourcesKeys.BadRequest]);
        }
        public async Task<Response<string>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _authorizationService.DeleteRoleAsync(request.Id);

            if (result == "NotFound")
            {
                return NotFound<string>(_localizer[SharedResourcesKeys.NotFound]);
            }
            else if (result == "HasUsers")
            {
                // Return 400 BadRequest with our specific protective message
                return BadRequest<string>(_localizer[SharedResourcesKeys.RoleHasUsers]);
            }
            else if (result == "Success")
            {
                return Success<string>(_localizer[SharedResourcesKeys.Deleted]);
            }

            return BadRequest<string>(_localizer[SharedResourcesKeys.BadRequest]);
        }
        public async Task<Response<string>> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            // Map the Core DTOs into a list of primitive Tuples
            var mappedRoles = request.UserRoles
                .Select(x => (RoleName: x.Name, HasRole: x.HasRole))
                .ToList();

            // Pass the clean Tuples to the Service
            var result = await _authorizationService.UpdateUserRolesAsync(request.UserId, mappedRoles);

            // Handle the response
            switch (result)
            {
                case "UserNotFound":
                    return NotFound<string>(_localizer[SharedResourcesKeys.UserNotFound]);
                case "FailedToRemoveOldRoles":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToRemoveOldRoles]);
                case "FailedToAddNewRoles":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToAddNewRoles]);
                case "FailedToUpdateRoles":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToUpdateRoles]);
                case "Success":
                    return Success<string>(_localizer[SharedResourcesKeys.Updated]);
                default:
                    return BadRequest<string>(_localizer[SharedResourcesKeys.BadRequest]);
            }
        }
        public async Task<Response<string>> Handle(UpdateUserClaimsCommand request, CancellationToken cancellationToken)
        {
            var result = await _authorizationService.UpdateUserClaimsAsync(request.UserId, request.UserClaims);

            switch (result)
            {
                case "Success":
                    return Success<string>(_localizer[SharedResourcesKeys.Success]);

                case "UserNotFound":
                    return NotFound<string>(_localizer[SharedResourcesKeys.UserNotFound]);

                case "FailedToRemove":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToRemovePermission]);

                case "FailedToAdd":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToAddPermission]);

                case "FailedToUpdate":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToUpdatePermissions]);

                default:
                    return BadRequest<string>(_localizer[SharedResourcesKeys.BadRequest]);
            }
        }
        public async Task<Response<string>> Handle(UpdateRoleClaimsCommand request, CancellationToken cancellationToken)
        {
            // Call the transactional service method
            var result = await _authorizationService.UpdateRoleClaimsAsync(request.RoleId, request.RoleClaims);


            switch (result)
            {
                case "Success":
                    return Success<string>(_localizer[SharedResourcesKeys.RoleClaimsUpdatedSuccessfully]);

                case "RoleNotFound":
                    return NotFound<string>(_localizer[SharedResourcesKeys.RoleNotExist]);

                case "FailedToRemove":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToRemovePermission]);

                case "FailedToAdd":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToAddPermission]);

                case "FailedToUpdate":
                    return BadRequest<string>(_localizer[SharedResourcesKeys.FailedToUpdatePermissions]);

                default:
                    // Fallback for any unexpected string returns
                    return BadRequest<string>(_localizer[SharedResourcesKeys.BadRequest]);
            }

        }
        public async Task<Response<string>> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
        {
            var result = await _authorizationService.ChangeUserStatusAsync(request.UserId, request.IsActive);

            switch (result)
            {
                case "Success":
                    // "User status has been successfully updated."
                    return Success<string>(_localizer[SharedResourcesKeys.Success]);

                case "UserNotFound":
                    return NotFound<string>(_localizer[SharedResourcesKeys.UserNotFound]);

                case "FailedToUpdateStatus":
                    // "Failed to update user status. Please try again."
                    return BadRequest<string>(_localizer[SharedResourcesKeys.BadRequest]);

                default:
                    return BadRequest<string>(_localizer[SharedResourcesKeys.BadRequest]);
            }
        }
    }
}
