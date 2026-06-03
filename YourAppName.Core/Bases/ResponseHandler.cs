using Microsoft.Extensions.Localization;
using YourAppName.Shared.Resources; // Make sure this points to where your SharedResources and Keys live
using System.Net;

namespace YourAppName.Core.Bases
{
    public class ResponseHandler
    {
        private readonly IStringLocalizer<SharedResources> _stringLocalizer;

        public ResponseHandler(IStringLocalizer<SharedResources> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public Response<T> Deleted<T>(string? message = null, object? Meta = null)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.OK,
                Succeeded = true,
                Message = message ?? _stringLocalizer[SharedResourcesKeys.Deleted], // if message is null, use the localized string
                Meta = Meta
            };
        }

        public Response<T> Success<T>(T entity, object? Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = HttpStatusCode.OK,
                Succeeded = true,
                Message = _stringLocalizer[SharedResourcesKeys.Success],
                Meta = Meta
            };
        }

        public Response<T> Unauthorized<T>(string? message = null, object? Meta = null)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Succeeded = false,
                Message = message ?? _stringLocalizer[SharedResourcesKeys.Unauthorized],
                Meta = Meta
            };
        }

        public Response<T> BadRequest<T>(string? message = null)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = message ?? _stringLocalizer[SharedResourcesKeys.BadRequest]
            };
        }

        public Response<T> UnprocessableEntity<T>(string? message = null)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.UnprocessableEntity,
                Succeeded = false,
                Message = message ?? _stringLocalizer[SharedResourcesKeys.UnprocessableEntity]
            };
        }

        public Response<T> NotFound<T>(string? message = null)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.NotFound,
                Succeeded = false,
                Message = message ?? _stringLocalizer[SharedResourcesKeys.NotFound]
            };
        }

        public Response<T> Created<T>(T entity, object? Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = HttpStatusCode.Created,
                Succeeded = true,
                Message = _stringLocalizer[SharedResourcesKeys.Created],
                Meta = Meta
            };
        }
    }
}