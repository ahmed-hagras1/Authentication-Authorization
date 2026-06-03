using MediatR;
using YourAppName.Core.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Auth.Commands.Models
{
    public class ForgotPasswordCommand : IRequest<Response<string>>
    {
        [Required]
        public string EmailOrPhone { get; set; } = string.Empty;
    }
}
