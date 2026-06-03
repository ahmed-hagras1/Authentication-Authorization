using MediatR;
using YourAppName.Core.Bases;
using YourAppName.Data.Results.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Commands.Models
{
    public class UpdateRoleClaimsCommand : IRequest<Response<string>>
    {
        public string RoleId { get; set; } = string.Empty;
        public List<RoleClaimDto> RoleClaims { get; set; } = new List<RoleClaimDto>();
    }
}
