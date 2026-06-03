using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using YourAppName.Core.Bases;
using YourAppName.Core.Features.Authorization.Queries.Models;
using YourAppName.Core.Features.Authorization.Queries.Results;
using YourAppName.Data.Results.Authorization;
using YourAppName.Service.Abstracts;
using YourAppName.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Queries.Handlers
{
    public class AuthorizationQueryHandler : ResponseHandler,
        IRequestHandler<GetRoleListQuery, Response<List<GetRoleListResult>>>,
        IRequestHandler<GetRoleByIdQuery, Response<GetRoleByIdResult>>,
        IRequestHandler<ManageUserRolesQuery, Response<ManageUserRolesResult>>,
        IRequestHandler<ManageUserClaimsQuery, Response<ManageUserClaimsResult>>,
        IRequestHandler<ManageRoleClaimsQuery, Response<ManageRoleClaimsResult>>
        
    {
        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly IMapper _mapper;
        #endregion
        #region Constructor
        public AuthorizationQueryHandler(IAuthorizationService authorizationService,
            IStringLocalizer<SharedResources> localizer,
            IMapper mapper) : base(localizer)
        {
            _authorizationService = authorizationService;
            _localizer = localizer;
            _mapper = mapper;
        }
        #endregion
        #region Handlers
        public async Task<Response<List<GetRoleListResult>>> Handle(GetRoleListQuery request, CancellationToken cancellationToken)
        {
            // Get roles from the service
            var roles = await _authorizationService.GetRolesListAsync(cancellationToken);

            // Map to the DTO result.
            var mappedRoles = _mapper.Map<List<GetRoleListResult>>(roles);

            // Return the standard Success response
            return Success(mappedRoles);
        }

        public async Task<Response<GetRoleByIdResult>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            // Fetch the role from the database
            var role = await _authorizationService.GetRoleByIdAsync(request.Id,cancellationToken);

            // Handle the Not Found case securely
            if (role == null)
            {
                return NotFound<GetRoleByIdResult>(_localizer[SharedResourcesKeys.NotFound]);
            }

            // Map to DTO
            var result = _mapper.Map<GetRoleByIdResult>(role);

            // Return Success
            return Success(result);
        }
        public async Task<Response<ManageUserRolesResult>> Handle(ManageUserRolesQuery request, CancellationToken cancellationToken)
        {
            // Call the service to get the raw data Tuple
            var data = await _authorizationService.GetManageUserRolesDataAsync(request.UserId);

            // If null, the user doesn't exist
            if (data == null)
            {
                return NotFound<ManageUserRolesResult>(_localizer[SharedResourcesKeys.UserNotFound]);
            }

            // Map the raw data into our Core DTO
            var result = new ManageUserRolesResult
            {
                UserId = request.UserId,
                UserRoles = new List<UserRoleViewModel>()
            };

            foreach (var role in data.Value.Roles) // .Value is used because the Tuple is nullable (?)
            {
                result.UserRoles.Add(new UserRoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    // If the user's role list contains this role name, mark as true
                    HasRole = data.Value.UserRoles.Contains(role.Name)
                });
            }

            // Return the fully mapped object
            return Success(result);
        }
        public async Task<Response<ManageUserClaimsResult>> Handle(ManageUserClaimsQuery request, CancellationToken cancellationToken)
        {
            // 1. Call the service which now returns the fully built checklist
            var result = await _authorizationService.ManageUserClaimsAsync(request.UserId);

            // 2. If null, the user doesn't exist
            if (result == null)
            {
                return NotFound<ManageUserClaimsResult>(_localizer[SharedResourcesKeys.UserNotFound]);
            }

            // 3. Return success
            return Success(result);
        }

        public async Task<Response<ManageRoleClaimsResult>> Handle(ManageRoleClaimsQuery request, CancellationToken cancellationToken)
        {
            var result = await _authorizationService.ManageRoleClaimsAsync(request.RoleId);

            if (result == null)
            {
                return NotFound<ManageRoleClaimsResult>(_localizer[SharedResourcesKeys.RoleNotExist]);
            }

            return Success(result);
        }
        #endregion
    }
}
