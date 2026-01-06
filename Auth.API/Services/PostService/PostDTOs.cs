using System;
using System.ComponentModel.DataAnnotations;

namespace Auth.API.Services
{
    // Create Post DTO
    public class CreatePostDto
    {
        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required string Content { get; set; }

        public required string UserId { get; set; }
    }

    // Update Post DTO
    public class UpdatePostDto
    {
        public required string Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
    }

    // Response DTO for single post
    public class PostResponseDto
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Content { get; set; }
        public required string UserId { get; set; }

        public string? UserEmail { get; set; }
    }

    // Minimal DTO for list of posts
    public class PostListDto
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
    }

    // General response DTO for posts
    public class PostResponseMessageDto
    {
        public required string Message { get; set; }
        public int? StatusCode { get; set; } = 200;
        public object? Data { get; set; }
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
    public class PaginationInput
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
