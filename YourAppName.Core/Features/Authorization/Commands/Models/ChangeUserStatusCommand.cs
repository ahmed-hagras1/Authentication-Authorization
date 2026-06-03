using MediatR;
using YourAppName.Core.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Commands.Models
{
    public class ChangeUserStatusCommand : IRequest<Response<string>>
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
