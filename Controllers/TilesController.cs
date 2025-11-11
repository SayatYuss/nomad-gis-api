using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/my-tiles")]
[Authorize]
public class TilesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    private const string TileServerBaseUrl = "http://localhost:3000";

    public TilesController(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    [HttpGet("{z}/{x}/{y}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMyFogTile(int z, int x, int y)
    {
        // Ожидаем URL вида: /api/v1/my-tiles/{z}/{x}/{y}?token=...
        var token = Request.Query["token"].ToString();

        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Token is missing");
        }

        var userId = ValidateTokenAndGetUserId(token);
        if (userId == null)
        {
            return Unauthorized("Invalid Token");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var martinUrl = $"{TileServerBaseUrl}/function/get_user_fog/{userId}/{z}/{x}/{y}";

            var response = await client.GetAsync(martinUrl);

            if (!response.IsSuccessStatusCode)
            {
                return new EmptyResult();
            }

            var tileBytes = await response.Content.ReadAsByteArrayAsync();
            return File(tileBytes, "application/vnd.mapbox-vector-tile");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Internal tile server error");
        }
    }

    private string? ValidateTokenAndGetUserId(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = TokenValidationHelper.GetTokenValidationParameters(_config);

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? principal.FindFirstValue("sub")
                         ?? principal.FindFirstValue("id");

            return userId;
        }
        catch
        {
            return null;
        }
    }
}