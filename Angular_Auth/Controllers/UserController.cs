using Angular_Auth.Context;
using Angular_Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Angular_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbcontext _authContext;
        public UserController(AppDbcontext appDbcontext)
        {
            _authContext = appDbcontext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] Users UserObj)
        {
            if (UserObj == null)
                return BadRequest();
            var user = await _authContext.Users.
                FirstOrDefaultAsync(x => x.UserName == UserObj.UserName && x.password==UserObj.password);
            if (user == null)
                return NotFound(new {Message="User Not Found"});
            return Ok(new
            {
                Message ="Login Success!"
            });
        }
        [HttpPost("register")]  
        public async Task<IActionResult> RegisterUser([FromBody] Users UserObj) 
        {
            if (UserObj==null)
                return BadRequest();
            await _authContext.Users.AddAsync(UserObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Message="User Register!"
            });
        }
    }
}
