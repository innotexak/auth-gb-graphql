namespace Auth.API.Services.DmService
{
    public class User
    {

        public Guid? Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string Avatar { get; set; }
        public PrefereceDto Preferences { get; set; }

        public string Username { get; set; }
    }
}