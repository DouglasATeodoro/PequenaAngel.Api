using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using PequenaAngel.Api.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PequenaAngel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthorizationController(UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;    
        }

        [HttpGet]
        public ActionResult<string> Get() 
        {
            return "AuthorizationController  ::  Acess in : "
                + DateTime.Now.ToLongTimeString();
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody]UserDTO model)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (!result.Succeeded)
            {
                   return BadRequest(result.Errors); 
            }

            await _signInManager.SignInAsync(user, false);
            return Ok(GeraToken(model));
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody]UserDTO user)
        {
            //check is valid model?
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
            }

            //check credentialls by user and return a value
            var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Ok(GeraToken(user));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Login Inválido!");
                return BadRequest(ModelState);
            }
        }

        private UserTokenDTO GeraToken(UserDTO user) 
        {
            //Declaration by user
            var claims = new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName, user.Email),
                new Claim("meuPet","Gohan"),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //gera uma chave com base em um algoritomo simetrico
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            //gera a assinatura digital do token usando o algoritimo Hmac e a chave privada
            var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            //tempo expiracao token
            var expiration = DateTime.UtcNow.AddHours(double.Parse(_configuration["TokenConfiguration:ExpireHours"]));

            //classe que representa um token e gera o token
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["TokenConfiguration:Issuer"],
                audience: _configuration["TokenConfiguration:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
                );

            //retorna user com o token e infos
            return new UserTokenDTO()
            {
                Autenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                Message = "Token JWT ok!"
            };

        }
    }
}
