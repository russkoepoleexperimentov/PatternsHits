
namespace Core.Application.Dtos
{
    public class CreateCommentDto
    {
        public string Text { get; set; } = null!;
        public Guid? ParentCommentId { get; set; }
    }
}
