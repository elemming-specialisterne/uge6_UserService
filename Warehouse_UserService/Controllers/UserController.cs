using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse_UserService.Factories;
using Warehouse_UserService.Models;

namespace Warehouse_UserService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<List<User>>> Get(int amount = 1)
        {
            var userList = new List<User>();

            for (int i = 0; i < amount; i++)
            {
                userList.Add(UserFactory.RandomUser(i));
            }

            return Ok(userList);
        }

        [HttpGet("noauth")]
        public async Task<ActionResult<List<User>>> GetNoAuth(int amount = 1)
        {
            var userList = new List<User>();

            for (int i = 0; i < amount; i++)
            {
                userList.Add(UserFactory.RandomUser(i));
            }

            return Ok(userList);
        }
    }
}
