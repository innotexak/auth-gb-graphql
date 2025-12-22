using Auth.API.ErrorHandling.Exceptions;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.ErrorHandling
{
    public class GraphQLErrorFilter : IErrorFilter
    {
        private readonly string statusCode = "statusCode";

        public IError OnError(IError error)
        {
            // Known AppException
            if (error.Exception is AppException appEx)
            {
                return ErrorBuilder
                    .FromError(error)
                    .SetMessage(appEx.Message)
                    .SetExtension(statusCode, appEx.StatusCode)
                    .Build();
            }

            // EF Core error
            if (error.Exception is DbUpdateException)
            {
                return ErrorBuilder
                    .FromError(error)
                    .SetMessage("A database error occurred.")
                    .SetExtension(statusCode, 500)
                    .Build();
            }

            // Task cancellation
            if (error.Exception is TaskCanceledException ||
                error.Exception is OperationCanceledException)
            {
                return ErrorBuilder
                    .FromError(error)
                    .SetMessage("The operation was cancelled.")
                    .SetExtension(statusCode, 499)
                    .Build();
            }

            // HotChocolate authorization error
            if (error.Code == "AUTH_NOT_AUTHENTICATED")
            {
                return ErrorBuilder
                    .FromError(error)
                    .SetMessage("You must be logged in to access this resource.")
                    .SetExtension(statusCode, 401)
                    .Build();
            }

            if (error.Code == "AUTH_NOT_AUTHORIZED")
            {
                return ErrorBuilder
                    .FromError(error)
                    .SetMessage("You do not have permission to perform this action.")
                    .SetExtension(statusCode, 403)
                    .Build();
            }

            // Fallback for everything else
            return ErrorBuilder
                .FromError(error)
                .SetMessage("Something went wrong. Please try again later.")
                .SetExtension(statusCode, 500)
                .Build();
        }
    }
}
