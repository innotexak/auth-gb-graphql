    using Auth.API.Data;
    using Auth.API.Entities;
using Auth.API.ErrorHandling.Exceptions;
using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;


    namespace Auth.API.Services.PostService
    {
        public class PostDatasource
        {
            private readonly AuthDBContext _dbContext;

            public PostDatasource(AuthDBContext dbContext)
            {
                _dbContext = dbContext;
            }

            // CREATE
            public async Task<NormalResponseWithDataDto<PostResponseDto>> CreatePostAsync(CreatePostDto post)
            {
                // Find the user by Id
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == post.UserId);
                if (user == null)
                {
                   throw new Exception("User not found");
                }

                var payload = new Post
                {
                    UserId = user.Id,
                    Title = post.Title,
                    Description = post.Description,
                    Content = post.Content,
                    User = user
                };

                await _dbContext.Posts.AddAsync(payload);
                await _dbContext.SaveChangesAsync();

                return new NormalResponseWithDataDto<PostResponseDto>
                {
                    Message = "Post added successfully",
                    StatusCode = 201,
                    Data = new PostResponseDto
                    {
                        Id = payload.Id,
                        Title = payload.Title,
                        Description = payload.Description,
                        Content = payload.Content,
                        UserId = payload.UserId.ToString(),
                        UserEmail = user.Email
                    }
                };
            }

            // READ
            public async Task<Post?> GetPostByIdAsync(string id)
            {
            if (!Guid.TryParse(id, out var postId))
            {
                return null;
            }
            return await _dbContext.Posts.FindAsync(postId);
            }

        public async Task<PaginatedResult<Post>> GetAllPostsAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Posts.AsQueryable();

            var totalCount = await query.CountAsync();

            var posts = await query
                .Include(p => p.User)
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Post>
            {
                Items = posts,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }


        // UPDATE
        public async Task<Post?> UpdatePostAsync(UpdatePostDto updatedPost)
        {
            if (!Guid.TryParse(updatedPost.Id, out var postId))
            {
                return null; 
            }

            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null) return null;

            if (!string.IsNullOrWhiteSpace(updatedPost.Title))
                post.Title = updatedPost.Title;

            if (!string.IsNullOrWhiteSpace(updatedPost.Content))
                post.Content = updatedPost.Content;

            if (!string.IsNullOrWhiteSpace(updatedPost.Description))
                post.Description = updatedPost.Description;

            await _dbContext.SaveChangesAsync();
            return post;
        }

        // DELETE
        public async Task<bool> DeletePostAsync(string id)
            {
                var post = await _dbContext.Posts.FirstOrDefaultAsync(u=>u.Id.ToString() == id);
                if (post == null) return false;

                _dbContext.Posts.Remove(post);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }
    }
