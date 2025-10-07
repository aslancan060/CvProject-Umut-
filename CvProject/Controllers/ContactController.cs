using CvProject.DTOs;
using CvProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CvProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ContactDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _contactService.SaveAndSendAsync(dto);
            return result ? Ok(new { message = "Message sent successfully. Thank you! 🙌" })
                          : StatusCode(500, "Failed to send message");
        }
    }
}
