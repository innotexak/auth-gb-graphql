namespace Auth.API.ErrorHandling.Exceptions
{
    public class UnauthorizedException: AppException
    {
        public UnauthorizedException(string message) : base(message, 401) { }
    }
}
