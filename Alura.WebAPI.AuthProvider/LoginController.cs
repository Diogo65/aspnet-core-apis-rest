using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Alura.ListaLeitura.Seguranca;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Alura.ListaLeitura.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        //SignInManager é uma classe do Identity para fazer Autenticação
        private readonly SignInManager<Usuario> _signInManager;

        public LoginController(SignInManager<Usuario> signInManager)
        {
            _signInManager = signInManager;
        }

        //irá validar o usuário enviado e irá gerar o Token
        [HttpPost]
        public async Task<IActionResult> Token(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                //SignInManager é uma classe do Identity para fazer Autenticação
                var result = await _signInManager.PasswordSignInAsync(model.Login, model.Password, true, true);
                if (result.Succeeded)
                {
                    //JWt 3 partes
                    //cria token (header + payload >> direitos + signature)

                    //2° Parte - payload(claims-direitos)
                    var direitos = new[]
                    {
                        //sub, sujeito deste token é o login do usuario
                        new Claim(JwtRegisteredClaimNames.Sub, model.Login),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //Nome registrado(identificador unico para o token
                    };

                    //3°Parte-Signature-A chave tem que ser compartilhada com chave que está na configuração(startup)
                    //Criptografia das duas partes com uma senha de 256 bts
                    var chave = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("alura-webapi-authentication-valid"));
                    var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256); //redencial p gerar assinaturo do json web token, usando algoritmo de criptografia

                    //Criando Token
                    var token = new JwtSecurityToken(
                        issuer: "Alura.WebApp",
                        audience: "Postman",
                        claims: direitos,
                        signingCredentials: credenciais,
                        expires: DateTime.Now.AddMinutes(900) //quando o token vai expirar
                    );
                    //Token Gerado \o/ !!!

                    //Transforma o token em uma string
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    //Retorna o Token
                    return Ok(tokenString);
                }
                return Unauthorized(); //401
            }
            return BadRequest(); //400
        }
    }
}