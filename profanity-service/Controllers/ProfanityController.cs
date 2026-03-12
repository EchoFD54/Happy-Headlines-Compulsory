using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProfanityController : ControllerBase{
    private readonly ProfanityService _profanityService;

    public ProfanityController(ProfanityService profanityService){
        _profanityService = profanityService;
    }

    [HttpPost("check")]
    public async Task<IActionResult> CheckText([FromBody] string text){
        var containsProfanity = await _profanityService.ContainsProfanity(text);

        return Ok(new { containsProfanity });
    }

    [HttpGet]
    public async Task<IActionResult> GetWords(){
        return Ok(await _profanityService.GetAllWords());
    }

    [HttpPost]
    public async Task<IActionResult> AddWord([FromBody] ProfanityWord word){
        return Ok(await _profanityService.AddWord(word));
    }
}