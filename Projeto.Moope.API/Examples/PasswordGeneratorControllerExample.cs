using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Core.Interfaces.Utils;
using Projeto.Moope.Core.Utils;

namespace Projeto.Moope.API.Examples
{
    /// <summary>
    /// Exemplo de Controller demonstrando o uso do PasswordGenerator
    /// Este controller é apenas para demonstração - NÃO usar em produção
    /// </summary>
    [ApiController]
    [Route("api/examples/password")]
    public class PasswordGeneratorControllerExample : ControllerBase
    {
        private readonly IPasswordGenerator _passwordGenerator;

        public PasswordGeneratorControllerExample(IPasswordGenerator passwordGenerator)
        {
            _passwordGenerator = passwordGenerator;
        }

        /// <summary>
        /// Demonstra geração de senha básica
        /// </summary>
        [HttpGet("basica")]
        public IActionResult GerarSenhaBasica()
        {
            var senha = _passwordGenerator.GerarSenha();
            return Ok(new { 
                senha,
                descricao = "Senha padrão (12 caracteres, todos os tipos)",
                valida = _passwordGenerator.ValidarSenha(senha)
            });
        }

        /// <summary>
        /// Demonstra geração de senha simples para cliente
        /// </summary>
        [HttpGet("cliente")]
        public IActionResult GerarSenhaCliente()
        {
            var senha = _passwordGenerator.GerarSenhaSimples(8);
            return Ok(new { 
                senha,
                descricao = "Senha simples para cliente (8 caracteres, sem símbolos)",
                valida = _passwordGenerator.ValidarSenha(senha)
            });
        }

        /// <summary>
        /// Demonstra geração de senha forte para administrador
        /// </summary>
        [HttpGet("admin")]
        public IActionResult GerarSenhaAdmin()
        {
            var senha = _passwordGenerator.GerarSenhaForte(16);
            return Ok(new { 
                senha,
                descricao = "Senha forte para administrador (16 caracteres, máxima segurança)",
                valida = _passwordGenerator.ValidarSenha(senha)
            });
        }

        /// <summary>
        /// Demonstra geração de PIN numérico
        /// </summary>
        [HttpGet("pin")]
        public IActionResult GerarPin()
        {
            var pin = _passwordGenerator.GerarPin(6);
            return Ok(new { 
                pin,
                descricao = "PIN numérico (6 dígitos)",
                valida = pin.Length == 6 && pin.All(char.IsDigit)
            });
        }

        /// <summary>
        /// Demonstra geração de senha temporária
        /// </summary>
        [HttpGet("temporaria")]
        public IActionResult GerarSenhaTemporaria()
        {
            var senha = _passwordGenerator.GerarSenhaTemporaria(10);
            return Ok(new { 
                senha,
                descricao = "Senha temporária (10 caracteres, será alterada pelo usuário)",
                valida = _passwordGenerator.ValidarSenha(senha)
            });
        }

        /// <summary>
        /// Demonstra geração de API Key
        /// </summary>
        [HttpGet("api-key")]
        public IActionResult GerarApiKey()
        {
            var apiKey = _passwordGenerator.GerarSenhaSeguraParaAPI(32);
            return Ok(new { 
                apiKey,
                descricao = "API Key segura (32 caracteres, sem caracteres problemáticos)",
                valida = _passwordGenerator.ValidarSenha(apiKey)
            });
        }

        /// <summary>
        /// Demonstra geração em lote
        /// </summary>
        [HttpGet("lote/{quantidade:int}")]
        public IActionResult GerarSenhasEmLote(int quantidade)
        {
            if (quantidade <= 0 || quantidade > 10)
            {
                return BadRequest("Quantidade deve estar entre 1 e 10 para este exemplo");
            }

            var senhas = _passwordGenerator.GerarSenhas(quantidade, PasswordPresets.ParaCliente);
            return Ok(new { 
                senhas = senhas.Select(s => new { 
                    senha = s, 
                    valida = _passwordGenerator.ValidarSenha(s) 
                }),
                descricao = $"{quantidade} senhas geradas usando preset para cliente"
            });
        }

        /// <summary>
        /// Demonstra configuração personalizada
        /// </summary>
        [HttpPost("personalizada")]
        public IActionResult GerarSenhaPersonalizada([FromBody] CustomPasswordRequest request)
        {
            try
            {
                var opcoes = new PasswordOptions
                {
                    Comprimento = request.Comprimento,
                    IncluirMinusculas = request.IncluirMinusculas,
                    IncluirMaiusculas = request.IncluirMaiusculas,
                    IncluirNumeros = request.IncluirNumeros,
                    IncluirSimbolos = request.IncluirSimbolos,
                    ExcluirAmbiguos = request.ExcluirAmbiguos,
                    GarantirDiversidade = request.GarantirDiversidade,
                    SimbolosCustomizados = request.SimbolosCustomizados
                };

                var senha = _passwordGenerator.GerarSenha(opcoes);
                return Ok(new { 
                    senha,
                    opcoes,
                    descricao = "Senha gerada com configuração personalizada",
                    valida = _passwordGenerator.ValidarSenha(senha)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Demonstra validação de senha
        /// </summary>
        [HttpPost("validar")]
        public IActionResult ValidarSenha([FromBody] ValidatePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Senha))
            {
                return BadRequest("Senha é obrigatória");
            }

            var valida = _passwordGenerator.ValidarSenha(request.Senha);
            var analise = AnalysarSenha(request.Senha);

            return Ok(new { 
                senha = request.Senha,
                valida,
                analise,
                criterios = new {
                    comprimentoMinimo = request.Senha.Length >= 6,
                    temMinuscula = request.Senha.Any(char.IsLower),
                    temMaiuscula = request.Senha.Any(char.IsUpper),
                    temNumero = request.Senha.Any(char.IsDigit),
                    temSimbolo = request.Senha.Any(c => "!@#$%&*+-=?".Contains(c))
                }
            });
        }

        /// <summary>
        /// Demonstra comparação de diferentes tipos
        /// </summary>
        [HttpGet("comparacao")]
        public IActionResult CompararTiposSenhas()
        {
            var exemplos = new[]
            {
                new { 
                    tipo = "Cliente", 
                    senha = _passwordGenerator.GerarSenhaSimples(8),
                    preset = "PasswordPresets.ParaCliente"
                },
                new { 
                    tipo = "Administrador", 
                    senha = _passwordGenerator.GerarSenhaForte(16),
                    preset = "PasswordPresets.ParaAdministrador"
                },
                new { 
                    tipo = "Temporária", 
                    senha = _passwordGenerator.GerarSenhaTemporaria(10),
                    preset = "PasswordPresets.Temporaria"
                },
                new { 
                    tipo = "PIN", 
                    senha = _passwordGenerator.GerarPin(6),
                    preset = "PasswordPresets.Pin"
                },
                new { 
                    tipo = "API", 
                    senha = _passwordGenerator.GerarSenha(PasswordPresets.ParaAPI),
                    preset = "PasswordPresets.ParaAPI"
                }
            };

            return Ok(new { 
                exemplos = exemplos.Select(e => new {
                    e.tipo,
                    e.senha,
                    comprimento = e.senha.Length,
                    valida = _passwordGenerator.ValidarSenha(e.senha),
                    preset = e.preset,
                    analise = AnalysarSenha(e.senha)
                }),
                descricao = "Comparação de diferentes tipos de senhas"
            });
        }

        #region Métodos Auxiliares

        private object AnalysarSenha(string senha)
        {
            return new
            {
                comprimento = senha.Length,
                temMinuscula = senha.Any(char.IsLower),
                temMaiuscula = senha.Any(char.IsUpper),
                temNumero = senha.Any(char.IsDigit),
                temSimbolo = senha.Any(c => "!@#$%&*+-=?".Contains(c)),
                tiposPresentes = new[] { 
                    senha.Any(char.IsLower),
                    senha.Any(char.IsUpper),
                    senha.Any(char.IsDigit),
                    senha.Any(c => "!@#$%&*+-=?".Contains(c))
                }.Count(x => x),
                forca = ClassificarForcaSenha(senha)
            };
        }

        private string ClassificarForcaSenha(string senha)
        {
            if (senha.Length < 6) return "Muito Fraca";
            
            var tipos = new[] { 
                senha.Any(char.IsLower),
                senha.Any(char.IsUpper),
                senha.Any(char.IsDigit),
                senha.Any(c => "!@#$%&*+-=?".Contains(c))
            }.Count(x => x);

            return (senha.Length, tipos) switch
            {
                (< 8, < 3) => "Fraca",
                (< 8, >= 3) => "Média",
                (< 12, < 3) => "Média",
                (< 12, >= 3) => "Forte",
                (>= 12, >= 3) => "Muito Forte",
                _ => "Fraca"
            };
        }

        #endregion

        #region DTOs

        public class CustomPasswordRequest
        {
            public int Comprimento { get; set; } = 12;
            public bool IncluirMinusculas { get; set; } = true;
            public bool IncluirMaiusculas { get; set; } = true;
            public bool IncluirNumeros { get; set; } = true;
            public bool IncluirSimbolos { get; set; } = true;
            public bool ExcluirAmbiguos { get; set; } = false;
            public bool GarantirDiversidade { get; set; } = true;
            public string? SimbolosCustomizados { get; set; }
        }

        public class ValidatePasswordRequest
        {
            public string Senha { get; set; } = string.Empty;
        }

        #endregion
    }
}
