namespace Auth.API.ErrorHandling.Exceptions
{
    public class UnprecessedEntityException:AppException
    {
        public UnprecessedEntityException(string message) : base(message, 422) { }
    }
}
