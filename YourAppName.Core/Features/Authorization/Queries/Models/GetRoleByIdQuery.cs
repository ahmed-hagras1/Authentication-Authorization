using MediatR;
using YourAppName.Core.Bases;
using YourAppName.Core.Features.Authorization.Queries.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Queries.Models
{
    public class GetRoleByIdQuery : IRequest<Response<GetRoleByIdResult>>
    {
        public string Id { get; set; }

        public GetRoleByIdQuery(string id)
        {
            Id = id;
        }
    }
}
