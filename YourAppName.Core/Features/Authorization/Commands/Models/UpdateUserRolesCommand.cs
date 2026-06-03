using MediatR;
using YourAppName.Core.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Commands.Models
{
    public class UpdateUserRolesCommand : IRequest<Response<string>>
    {
        public string UserId { get; set; } = string.Empty;
        public List<UpdateUserRoleRequest> UserRoles { get; set; } = new();
    }

    public class UpdateUserRoleRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool HasRole { get; set; }
    }
}
