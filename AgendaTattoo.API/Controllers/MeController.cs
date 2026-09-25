using AgendaTattoo.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly ICurrentUserService _currentUser;

        public MeController(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                userId = _currentUser.UserId,
                studioId = _currentUser.StudioId,
                roles = _currentUser.Roles,
            });
        }
    }
}
