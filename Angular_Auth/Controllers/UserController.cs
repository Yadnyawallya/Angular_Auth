using Angular_Auth.Context;
using Angular_Auth.Helper;
using Angular_Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;


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
                //FirstOrDefaultAsync(x => x.UserName == UserObj.UserName && x.password==UserObj.password);     
                FirstOrDefaultAsync(x => x.UserName == UserObj.UserName);

            if (user == null)
                return NotFound(new { Message = "User Not Found!" });


            if (!PasswordHasher.VerifyPassword(UserObj.password, user.password))
            {
                return BadRequest(new { Messgage = "password is Incorrect" });

            }


            user.Token = CreateJWT(user);

            return Ok(new
            {
                Token = user.Token,

                Message = "Login Success!"
            });
        }
        [HttpPost("register")]  
        public async Task<IActionResult> RegisterUser([FromBody] Users UserObj) 
        {
            if (UserObj==null)
                return BadRequest();
            //check Username
            if (await CheckUserNameExistAsync(UserObj.UserName)) 
                return BadRequest(new { Message = " user already Exists" });

            //check Email 
            if (await CheckEmailExistAsync(UserObj.Email))
                return BadRequest(new { Message = " Email already Exists" });

            //check Password Strength
            var pass = CheckPasswordStrength(UserObj.password);
            if (!string.IsNullOrEmpty(pass))
                return BadRequest(new { Message = pass.ToString()});

            UserObj.password = PasswordHasher.HashPassword(UserObj.password);
            UserObj.Role = "user";
            UserObj.Token = "";
            await _authContext.Users.AddAsync(UserObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            { 
                Message="User Register!"
            });
        }

        private  Task<bool> CheckUserNameExistAsync(string UserName) 
            => _authContext.Users.AnyAsync(x => x.UserName == UserName);

        private Task<bool> CheckEmailExistAsync(string Email)
            => _authContext.Users.AnyAsync(x => x.Email == Email);
        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sb.Append("password should be alphanumeric" + Environment.NewLine);
            if (!Regex.IsMatch(password, "[<,>,!,@,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("password should contain Special characters" + Environment.NewLine);
            return sb.ToString();
            
        }

        private string CreateJWT(Users user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverySecret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.FirstName}{user.LastName}")

            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var toekn = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(toekn);
        }


    }
}
