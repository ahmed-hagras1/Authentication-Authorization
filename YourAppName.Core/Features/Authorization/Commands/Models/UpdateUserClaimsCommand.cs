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
    public class UpdateUserClaimsCommand : IRequest<Response<string>>
    {
        public string UserId { get; set; } = string.Empty;
        public List<UserClaimDto> UserClaims { get; set; } = new List<UserClaimDto>();
    }
}
