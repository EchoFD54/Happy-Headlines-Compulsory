using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DraftController : ControllerBase{
    private readonly DraftService _draftService;

    public DraftController(DraftService draftService){
        _draftService = draftService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDraft([FromBody] Draft draft){
        var created = await _draftService.CreateDraft(draft);
        return Ok(created);
    }

   
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDrafts(Guid id){
        var drafts = await _draftService.GetDrafts(id);
        if (drafts == null)
            return NotFound();
        return Ok(drafts);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDraft(Guid id, [FromBody] Draft draft){
        var updated = await _draftService.UpdateDraft(id, draft);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDraft(Guid id){
        var success = await _draftService.DeleteDraft(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
}