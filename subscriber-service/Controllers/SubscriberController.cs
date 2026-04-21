using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SubscriberController : ControllerBase {
    private readonly SubscriberService _service;

    public SubscriberController(SubscriberService service) {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request) { 
        if (!_service.IsEnabled()) {
            return StatusCode(503, "Subscriber service is temporarily disabled.");
        }

        await _service.SubscribeAsync(request.Email);
        return Ok("Subscribed successfully");
    }
}

public class SubscribeRequest
{
    public string Email { get; set; } = string.Empty;
}