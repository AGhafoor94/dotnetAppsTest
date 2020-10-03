using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/Users")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _db;
        public UsersController(DataContext db)
        {
            _db = db;
        }
        [HttpGet]
        public async Task<ActionResult<IList<AppUser>>> GetUsers()
        {
            try
            {
                var users = await _db.Users.ToListAsync();
                return Ok(users);
            }
            catch (System.Exception e)
            {
                return StatusCode(500, $"{e.InnerException} - {e.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            try
            {
                var user = await _db.Users.FindAsync(id);
                return Ok(user);
            }
            catch (System.Exception e)
            {
                return StatusCode(500, $"{e.InnerException} - {e.Message}");
            }
        }
    }
}