using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _db;
        private readonly ITokenService _token;
        public AccountController(DataContext db, ITokenService token)
        {
            _db = db;
            _token = token;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
        {
            try
            {
                if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
                using var hmac = new HMACSHA512();
                var user = new AppUser
                {
                    UserName = registerDto.Username.ToLower(),
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                    PasswordSalt = hmac.Key
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return Ok(new UserDTO
                {
                    Username = user.UserName,
                    Token = _token.CreateToken(user)
                });
            }
            catch (System.Exception e)
            {
                return StatusCode(500, $"{e.Message}");
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
        {
            try
            {
                var user = await _db.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);
                if (user == null) return Unauthorized("Invalid username");
                using var hmac = new HMACSHA512(user.PasswordSalt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
                }
                return Ok(new UserDTO
                {
                    Username = user.UserName,
                    Token = _token.CreateToken(user)
                });
            }
            catch (System.Exception e)
            {
                return StatusCode(500, $"{e.Message}");
            }
        }
        private async Task<bool> UserExists(string username)
        {
            return await _db.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}