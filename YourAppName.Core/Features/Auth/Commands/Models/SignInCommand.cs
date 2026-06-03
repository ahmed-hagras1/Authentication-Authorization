using MediatR;
using YourAppName.Core.Bases;
using YourAppName.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Auth.Commands.Models
{
    public class SignInCommand : IRequest<Response<JWTAuthResult>>
    {
        // This can be an Email or a Phone Number
        public string LoginIdentifier { get; set; }
        public string Password { get; set; }
    }
}
