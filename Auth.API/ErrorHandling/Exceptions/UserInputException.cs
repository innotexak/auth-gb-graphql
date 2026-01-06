namespace Auth.API.ErrorHandling.Exceptions
{
    public class UserInputException:AppException
    {
        public UserInputException(string message) : base(message, 400) { }
    }
}
