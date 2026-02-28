
namespace Core.Application.Dtos
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ParentCommentId { get; set; }
        public List<CommentDto>? Replies { get; set; }
    }
}
