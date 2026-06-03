using MediatR;
using YourAppName.Core.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Auth.Commands.Models
{
    public class VerifyCodeCommand : IRequest<Response<string>>
    {
        public string EmailOrPhone { get; set; } = null!;
        public string Code { get; set; } = null!; // The 6-digit OTP
    }
}
