using Auth.API.Entities;
using HotChocolate;
using Auth.API.Services.PostService;

namespace Auth.API.Services.PostService
{
    [ExtendObjectType(Name = "Mutation")]
    public class PostMutation
    {
        public Task<NormalResponseWithDataDto<PostResponseDto>> CreatePost(CreatePostDto input, [Service] PostDatasource postDatasource)
        {
            return postDatasource.CreatePostAsync(input);
        }

        public Task<Post?> UpdatePost( UpdatePostDto input, [Service] PostDatasource postDatasource)
        {
            return postDatasource.UpdatePostAsync( input);
        }

        public Task<bool> DeletePost(string id, [Service] PostDatasource postDatasource)
        {
            return postDatasource.DeletePostAsync(id);
        }
    }
}
