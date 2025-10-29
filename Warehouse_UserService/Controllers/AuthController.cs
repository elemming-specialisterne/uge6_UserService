using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Warehouse_UserService.Extensions;
using Warehouse_UserService.Models;

namespace Warehouse_UserService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : Controller
    {


        private readonly IHttpClientFactory _http;
        private readonly TokenGenerator _tokenGenerator;

        public AuthController(IHttpClientFactory http, TokenGenerator tokenGenerator)
        {
            _http = http;
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
                //UserId = userCreationRequest.Email,
                UserId = user.Id,
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
        public async Task<ActionResult<string>> Login([FromBody]JsonElement body)
        {

            if (!body.TryGetProperty("username", out var u) || u.ValueKind != JsonValueKind.String)
                return BadRequest("Missing username");

            if (!body.TryGetProperty("password", out var p) || p.ValueKind != JsonValueKind.String)
                return BadRequest("Missing password");

            var payload = new 
            { 
                identifier = u.GetString(),
                pass = p.GetString()
            };

            // http request to db
            var client = _http.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, "http://localhost:3000/rpc/login")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };


            // send request
            var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

            // map 401/403 to unauthrorized
            if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) return Unauthorized();

            // bubble up other non-sucess with raw body
            if (!res.IsSuccessStatusCode) return StatusCode((int)res.StatusCode, await res.Content.ReadAsStringAsync());


            // parse postgrest json
            using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            // read files
            var token = root.TryGetProperty("token", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
            var role = root.TryGetProperty("role", out var r) && r.ValueKind == JsonValueKind.String ? r.GetString() : null;
            var uid = root.TryGetProperty("user_id", out var id) && id.TryGetInt64(out var v) ? v : (long?)null;

            // require token
            if (string.IsNullOrWhiteSpace(token)) return Unauthorized();

            var refreshToken = Guid.NewGuid().ToString(); // Placeholder for refresh token generation
            var refreshTokenIdentity = new RefreshToken
            {
                Token = refreshToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                Revoked = false
            };

            return Ok(new
            {
               access_token = token, refresh_token = refreshTokenIdentity,
               user_id = uid,
               role

            });
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
