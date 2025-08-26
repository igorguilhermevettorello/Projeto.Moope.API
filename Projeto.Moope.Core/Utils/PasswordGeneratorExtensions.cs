using Projeto.Moope.Core.Interfaces.Utils;

namespace Projeto.Moope.Core.Utils
{
    /// <summary>
    /// Extensões e métodos de conveniência para o PasswordGenerator
    /// </summary>
    public static class PasswordGeneratorExtensions
    {
        /// <summary>
        /// Gera uma senha simples apenas com letras e números (sem símbolos)
        /// </summary>
        public static string GerarSenhaSimples(this IPasswordGenerator generator, int comprimento = 8)
        {
            return generator.GerarSenha(new PasswordOptions
            {
                Comprimento = comprimento,
                IncluirMinusculas = true,
                IncluirMaiusculas = true,
                IncluirNumeros = true,
                IncluirSimbolos = false,
                ExcluirAmbiguos = true,
                GarantirDiversidade = true
            });
        }

        /// <summary>
        /// Gera uma senha forte com todos os tipos de caracteres
        /// </summary>
        public static string GerarSenhaForte(this IPasswordGenerator generator, int comprimento = 16)
        {
            return generator.GerarSenha(new PasswordOptions
            {
                Comprimento = comprimento,
                IncluirMinusculas = true,
                IncluirMaiusculas = true,
                IncluirNumeros = true,
                IncluirSimbolos = true,
                ExcluirAmbiguos = false,
                GarantirDiversidade = true
            });
        }

        /// <summary>
        /// Gera uma senha numérica (PIN)
        /// </summary>
        public static string GerarPin(this IPasswordGenerator generator, int comprimento = 6)
        {
            return generator.GerarSenha(new PasswordOptions
            {
                Comprimento = comprimento,
                IncluirMinusculas = false,
                IncluirMaiusculas = false,
                IncluirNumeros = true,
                IncluirSimbolos = false,
                ExcluirAmbiguos = true,
                GarantirDiversidade = false
            });
        }

        /// <summary>
        /// Gera uma senha apenas com letras (sem números ou símbolos)
        /// </summary>
        public static string GerarSenhaAlfabetica(this IPasswordGenerator generator, int comprimento = 10)
        {
            return generator.GerarSenha(new PasswordOptions
            {
                Comprimento = comprimento,
                IncluirMinusculas = true,
                IncluirMaiusculas = true,
                IncluirNumeros = false,
                IncluirSimbolos = false,
                ExcluirAmbiguos = true,
                GarantirDiversidade = true
            });
        }

        /// <summary>
        /// Gera uma senha temporária (mais simples, fácil de digitar)
        /// </summary>
        public static string GerarSenhaTemporaria(this IPasswordGenerator generator, int comprimento = 8)
        {
            return generator.GerarSenha(new PasswordOptions
            {
                Comprimento = comprimento,
                IncluirMinusculas = true,
                IncluirMaiusculas = true,
                IncluirNumeros = true,
                IncluirSimbolos = false,
                ExcluirAmbiguos = true,
                GarantirDiversidade = true,
                SimbolosCustomizados = null
            });
        }

        /// <summary>
        /// Gera uma senha com símbolos seguros (evita caracteres problemáticos em URLs/JSON)
        /// </summary>
        public static string GerarSenhaSeguraParaAPI(this IPasswordGenerator generator, int comprimento = 12)
        {
            return generator.GerarSenha(new PasswordOptions
            {
                Comprimento = comprimento,
                IncluirMinusculas = true,
                IncluirMaiusculas = true,
                IncluirNumeros = true,
                IncluirSimbolos = true,
                ExcluirAmbiguos = true,
                GarantirDiversidade = true,
                SimbolosCustomizados = "!@#$%&*+-=" // Evita caracteres como ? / \ que podem causar problemas
            });
        }
    }

    /// <summary>
    /// Classe utilitária com configurações pré-definidas
    /// </summary>
    public static class PasswordPresets
    {
        /// <summary>
        /// Configuração para senha de cliente (fácil de lembrar)
        /// </summary>
        public static PasswordOptions ParaCliente => new()
        {
            Comprimento = 8,
            IncluirMinusculas = true,
            IncluirMaiusculas = true,
            IncluirNumeros = true,
            IncluirSimbolos = false,
            ExcluirAmbiguos = true,
            GarantirDiversidade = true
        };

        /// <summary>
        /// Configuração para senha de administrador (máxima segurança)
        /// </summary>
        public static PasswordOptions ParaAdministrador => new()
        {
            Comprimento = 16,
            IncluirMinusculas = true,
            IncluirMaiusculas = true,
            IncluirNumeros = true,
            IncluirSimbolos = true,
            ExcluirAmbiguos = false,
            GarantirDiversidade = true
        };

        /// <summary>
        /// Configuração para senha temporária (será alterada pelo usuário)
        /// </summary>
        public static PasswordOptions Temporaria => new()
        {
            Comprimento = 10,
            IncluirMinusculas = true,
            IncluirMaiusculas = true,
            IncluirNumeros = true,
            IncluirSimbolos = false,
            ExcluirAmbiguos = true,
            GarantirDiversidade = true
        };

        /// <summary>
        /// Configuração para PIN numérico
        /// </summary>
        public static PasswordOptions Pin => new()
        {
            Comprimento = 6,
            IncluirMinusculas = false,
            IncluirMaiusculas = false,
            IncluirNumeros = true,
            IncluirSimbolos = false,
            ExcluirAmbiguos = true,
            GarantirDiversidade = false
        };

        /// <summary>
        /// Configuração para senha de API/sistema
        /// </summary>
        public static PasswordOptions ParaAPI => new()
        {
            Comprimento = 32,
            IncluirMinusculas = true,
            IncluirMaiusculas = true,
            IncluirNumeros = true,
            IncluirSimbolos = true,
            ExcluirAmbiguos = true,
            GarantirDiversidade = true,
            SimbolosCustomizados = "!@#$%&*+-="
        };
    }
}
