using MediatR;
using YourAppName.Core.Bases;
using YourAppName.Data.Results.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Queries.Models
{
    public class ManageRoleClaimsQuery : IRequest<Response<ManageRoleClaimsResult>>
    {
        public string RoleId { get; set; }

        public ManageRoleClaimsQuery(string roleId)
        {
            RoleId = roleId;
        }
    }
}
