    using Auth.API.Entities;
    using Auth.API.Services.PostService;
    using HotChocolate;


    namespace Auth.API.Services.PostService
    {
        
    [ExtendObjectType(Name = "Query")]
    public class PostQuery
    {
        public async Task<PaginatedResult<Post>> GetPosts(PaginationInput input , [Service] PostDatasource postDatasource)
        {
            return await postDatasource.GetAllPostsAsync(input.PageNumber, input.PageSize);
        }

        public async Task<Post?> GetPostById(string id, [Service] PostDatasource postDatasource)
        {
            Console.WriteLine(id, "identification");
            return await postDatasource.GetPostByIdAsync(id);
        }
    }
}