namespace Auth.API.ErrorHandling.Exceptions
{
    public class BadRequestException: AppException
    {
        public BadRequestException(string message) : base(message, 400) { }
    }
}
