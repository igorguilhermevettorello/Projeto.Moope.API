using Microsoft.AspNetCore.Mvc;

namespace Projeto.Moope.API.Controllers
{
    [Route("api/swagger-auth")]
    [ApiController]
    public class SwaggerAuthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            var html = @"<!DOCTYPE html>
                         <html lang=""en"">
                         <head>
                             <meta charset=""UTF-8"">
                             <title>Login Swagger</title>
                             <!-- Bootstrap 5 CDN -->
                             <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"">
                         </head>
                         <body class=""bg-light"">

                         <div class=""container mt-5"">
                             <div class=""row justify-content-center"">
                                 <div class=""col-md-6"">
                                     <div class=""card shadow-sm"">
                                         <div class=""card-header bg-primary text-white"">
                                             <h4 class=""mb-0"">Login Swagger</h4>
                                         </div>
                                         <div class=""card-body"">
                                             <form action=""/api/swagger-auth/login"" method=""post"">
                                                 <div class=""mb-3"">
                                                     <label for=""username"" class=""form-label"">Usuário</label>
                                                     <input type=""text"" class=""form-control"" id=""username"" name=""username"" required>
                                                 </div>
                                                 <div class=""mb-3"">
                                                     <label for=""password"" class=""form-label"">Senha</label>
                                                     <input type=""password"" class=""form-control"" id=""password"" name=""password"" required>
                                                 </div>
                                                 <button type=""submit"" class=""btn btn-primary w-100"">Entrar</button>
                                             </form>
                                         </div>
                                     </div>
                                 </div>
                             </div>
                         </div>

                         <!-- Bootstrap Bundle JS -->
                         <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js""></script>
                         </body>
                         </html>";

            return Content(html, "text/html");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
        {
            if (username == "admin" && password == "admin@123")
            {
                HttpContext.Response.Cookies.Append("SwaggerAuth", "true", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Sempre que possível, use HTTPS
                    Expires = DateTimeOffset.UtcNow.AddHours(2) // expira em 2 horas
                });

                var sucesso = @"<!DOCTYPE html>
                         <html lang=""en"">
                         <head>
                             <meta charset=""UTF-8"">
                             <title>Login Swagger</title>
                             <!-- Bootstrap 5 CDN -->
                             <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"">
                         </head>
                         <body class=""bg-light"">

                         <div class=""container mt-5"">
                             <div class=""row justify-content-center"">
                                 <div class=""col-md-6"">
                                     <div class=""card shadow-sm"">
                                         <div class=""card-header bg-primary text-white"">
                                             <h4 class=""mb-0"">Login</h4>
                                         </div>
                                         <div class=""card-body"">
                                             <div>Autenticado com sucesso! Agora acesse o Swagger..</div>
                                             <a type=""submit"" class=""btn btn-primary w-100"" href=""/swagger"">Acesse o swagger</a>
                                         </div>
                                     </div>
                                 </div>
                             </div>
                         </div>

                         <!-- Bootstrap Bundle JS -->
                         <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js""></script>
                         </body>
                         </html>";

                return Content(sucesso, "text/html");
            }

            var html = @"<!DOCTYPE html>
                         <html lang=""en"">
                         <head>
                             <meta charset=""UTF-8"">
                             <title>Login Swagger</title>
                             <!-- Bootstrap 5 CDN -->
                             <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"">
                         </head>
                         <body class=""bg-light"">

                         <div class=""container mt-5"">
                             <div class=""row justify-content-center"">
                                 <div class=""col-md-6"">
                                     <div class=""card shadow-sm"">
                                         <div class=""card-header bg-primary text-white"">
                                             <h4 class=""mb-0"">Erro Login</h4>
                                         </div>
                                         <div class=""card-body"">
                                             <div>Credenciais inválidas.</div>
                                             <a type=""submit"" class=""btn btn-primary w-100"" href=""/api/swagger-auth"">Tente novamente</a>
                                         </div>
                                     </div>
                                 </div>
                             </div>
                         </div>

                         <!-- Bootstrap Bundle JS -->
                         <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js""></script>
                         </body>
                         </html>";

            return Content(html, "text/html");
        }
    }
}
