using Microsoft.AspNetCore.Mvc;
using MoodleInstanceBridge.Models.Errors;

namespace MoodleInstanceBridge.Attributes
{
    /// <summary>
    /// Attribute to declare standardized error response types for Swagger documentation
    /// </summary>
    public class StandardErrorResponsesAttribute : ProducesResponseTypeAttribute
    {
        public StandardErrorResponsesAttribute() : base(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)
        {
        }
    }

    /// <summary>
    /// Attribute to declare validation error response for Swagger documentation
    /// </summary>
    public class ValidationErrorResponseAttribute : ProducesResponseTypeAttribute
    {
        public ValidationErrorResponseAttribute() : base(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)
        {
        }
    }

    /// <summary>
    /// Attribute to declare authentication error response for Swagger documentation
    /// </summary>
    public class AuthenticationErrorResponseAttribute : ProducesResponseTypeAttribute
    {
        public AuthenticationErrorResponseAttribute() : base(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)
        {
        }
    }

    /// <summary>
    /// Attribute to declare authorization error response for Swagger documentation
    /// </summary>
    public class AuthorizationErrorResponseAttribute : ProducesResponseTypeAttribute
    {
        public AuthorizationErrorResponseAttribute() : base(typeof(ErrorResponse), StatusCodes.Status403Forbidden)
        {
        }
    }

    /// <summary>
    /// Attribute to declare not found error response for Swagger documentation
    /// </summary>
    public class NotFoundErrorResponseAttribute : ProducesResponseTypeAttribute
    {
        public NotFoundErrorResponseAttribute() : base(typeof(ErrorResponse), StatusCodes.Status404NotFound)
        {
        }
    }

    /// <summary>
    /// Attribute to declare upstream error response for Swagger documentation
    /// </summary>
    public class UpstreamErrorResponseAttribute : ProducesResponseTypeAttribute
    {
        public UpstreamErrorResponseAttribute() : base(typeof(ErrorResponse), StatusCodes.Status502BadGateway)
        {
        }
    }

    /// <summary>
    /// Attribute to declare partial failure response for Swagger documentation
    /// </summary>
    public class PartialFailureErrorResponseAttribute : ProducesResponseTypeAttribute
    {
        public PartialFailureErrorResponseAttribute() : base(typeof(PartialFailureResponse), StatusCodes.Status207MultiStatus)
        {
        }
    }
}
