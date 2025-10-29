using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text;
using Warehouse_UserService.Extensions;
using Warehouse_UserService.Models;

namespace Warehouse_UserService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly TokenGenerator _tokenGenerator;

        public AuthController(TokenGenerator tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
        }

        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [HttpPost("create")]
        public async Task<ActionResult<string>> Create([FromBody] UserCreationRequest userCreationRequest)
        {
            // TODO: Create user in database here

            var errorReportSB = new StringBuilder();

            if (userCreationRequest.Username.IsNullOrEmptyOrWhiteSpace())
            {
                errorReportSB.AppendLine("Missing username.");
            }

            if (userCreationRequest.Name.IsNullOrEmptyOrWhiteSpace())
            {
                errorReportSB.AppendLine("Missing name.");
            }

            if (userCreationRequest.Lastname.IsNullOrEmptyOrWhiteSpace())
            {
                errorReportSB.AppendLine("Missing last name.");
            }

            if (userCreationRequest.Email.IsNullOrEmptyOrWhiteSpace())
            {
                errorReportSB.AppendLine("Missing email.");
            }
            else
            {
                try
                {
                    var mailAddress = new MailAddress(userCreationRequest.Email);
                }
                catch (FormatException e)
                {
                    errorReportSB.AppendLine("Invalid email format.");
                }
            }

            if (userCreationRequest.Password.IsNullOrEmptyOrWhiteSpace())
            {
                errorReportSB.AppendLine("Missing password.");
            }

            if (errorReportSB.Length > 0)
            {
                return BadRequest(errorReportSB.ToString());
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userCreationRequest.Password);

            var user = new User
            {
                Username = userCreationRequest.Username,
                Name = userCreationRequest.Name,
                Lastname = userCreationRequest.Lastname,
                Email = userCreationRequest.Email,
                Password = hashedPassword,
                Role = "User"
            };

            // TODO: Generate access token
            var token = _tokenGenerator.GenerateToken(user.Email, user.Role);

            var refreshToken = Guid.NewGuid().ToString(); // Placeholder for refresh token generation
            var refreshTokenIdentity = new RefreshToken
            {
                UserId = userCreationRequest.Email,
                Token = refreshToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                Revoked = false
            };

            return Ok(new {
                access_token = token,
                refresh_token = refreshTokenIdentity,
                userData = user
            });
        }

        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest loginRequest)
        {
            // TODO: Validate user credentials here

            // Generate token
            var token = _tokenGenerator.GenerateToken(loginRequest.Email, "Admin");

            var refreshToken = Guid.NewGuid().ToString(); // Placeholder for refresh token generation
            var refreshTokenIdentity = new RefreshToken
            {
                UserId = loginRequest.Email,
                Token = refreshToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                Revoked = false
            };

            return Ok(new { access_token = token, refresh_token = refreshTokenIdentity });
        }

        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status419AuthenticationTimeout)]
        [HttpPost]
        public async Task<ActionResult> RefreshToken([FromBody] TokenResponse accessCredentials)
        {
            return Ok();
        }
    }
}
