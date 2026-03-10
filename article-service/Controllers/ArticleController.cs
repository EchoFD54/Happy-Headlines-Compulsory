using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ArticleController : ControllerBase{
    private readonly ArticleService _articleService;

    public ArticleController(ArticleService articleService){
        _articleService = articleService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateArticle([FromBody] Article article){
        var created = await _articleService.CreateArticle(article);
        return CreatedAtAction(nameof(GetArticle), new { id = created.Id }, created);
    }

   
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetArticle(Guid id, [FromQuery] Continent continent){
        var article = await _articleService.GetArticle(id, continent);
        if (article == null)
            return NotFound();
        return Ok(article);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] Article article){
        var updated = await _articleService.UpdateArticle(id, article);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteArticle(Guid id, [FromQuery] Continent continent){
        var success = await _articleService.DeleteArticle(id, continent);
        if (!success)
            return NotFound();
        return NoContent();
    }
}