using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using YourAppName.Core.Bases;
using System.Net;

namespace YourAppName.Api.Base
{
    [ApiController]
    public class AppControllerBase : ControllerBase
    {
        private IMediator? _mediatorInstance;

        // 1. Smart Mediator Property
        // If _mediatorInstance is null, get it from HttpContext.Services. 
        // This means Child Controllers don't need a Constructor!
        protected IMediator Mediator => _mediatorInstance ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        // 2. The "Magic" Response Handler
        public ObjectResult NewResult<T>(Response<T> response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new OkObjectResult(response);

                case HttpStatusCode.Created:
                    return new CreatedResult(string.Empty, response);

                case HttpStatusCode.Unauthorized:
                    return new UnauthorizedObjectResult(response);

                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(response);

                case HttpStatusCode.NotFound:
                    return new NotFoundObjectResult(response);

                case HttpStatusCode.Accepted:
                    return new AcceptedResult(string.Empty, response);

                case HttpStatusCode.UnprocessableEntity:
                    return new UnprocessableEntityObjectResult(response);

                default:
                    return new BadRequestObjectResult(response);
            }
        }
    }
}