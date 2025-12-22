using Auth.API.Services;
using Auth.API.Services.DmService;
using Auth.API.Services.AuthService;
using Auth.API.Services.DmService;
using Auth.API.Services.PostService;


namespace Auth.API.Modules
{
    public class GraphQLModules
    {
        // Grouped Queries
        public Type[] Queries => new Type[]
        {
        typeof(AuthQuery),
        typeof(PostQuery),
        typeof(DmQuery),
            // Add more modules here
        };

        // Grouped Mutations
        public Type[] Mutations => new Type[]
        {
        typeof(AuthMutation),
        typeof(PostMutation),
        typeof(DmMutation),
            // Add more modules here
        };

        // Grouped Mutations
        public Type[] Subscriptions => new Type[]
        {
        typeof(DmSubscription)
  
            // Add more modules here
        };
    }
}

