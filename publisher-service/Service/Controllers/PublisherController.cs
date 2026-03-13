using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/publisher")]
public class PublisherController : ControllerBase{
    private readonly PublisherService _service;

    public PublisherController(PublisherService service){
        _service = service;
    }

    [HttpPost("publish")]
    public async Task<IActionResult> PublishArticle([FromBody] ArticleMessage article){
        await _service.PublishArticle(article);

        return Ok();
    }
}