using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase{
    private readonly CommentService _commentService;

    public CommentController(CommentService commentService){
        _commentService = commentService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] Comment comment){
        var created = await _commentService.CreateComment(comment);
        return Ok(created);
    }

    [HttpGet("{articleId:guid}")]
    public async Task<IActionResult> GetComments(Guid articleId){
        var comments = await _commentService.GetCommentsByArticle(articleId);
        return Ok(comments);
    }
}