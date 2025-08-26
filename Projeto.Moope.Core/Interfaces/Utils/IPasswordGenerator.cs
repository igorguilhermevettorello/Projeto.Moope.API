namespace Projeto.Moope.Core.Interfaces.Utils
{
    /// <summary>
    /// Interface para geração de senhas automáticas
    /// </summary>
    public interface IPasswordGenerator
    {
        /// <summary>
        /// Gera uma senha aleatória com configurações padrão
        /// </summary>
        /// <returns>Senha gerada</returns>
        string GerarSenha();

        /// <summary>
        /// Gera uma senha aleatória com comprimento específico
        /// </summary>
        /// <param name="comprimento">Comprimento da senha (mínimo 6, máximo 128)</param>
        /// <returns>Senha gerada</returns>
        string GerarSenha(int comprimento);

        /// <summary>
        /// Gera uma senha aleatória com configurações personalizadas
        /// </summary>
        /// <param name="opcoes">Opções de configuração da senha</param>
        /// <returns>Senha gerada</returns>
        string GerarSenha(PasswordOptions opcoes);

        /// <summary>
        /// Gera múltiplas senhas aleatórias
        /// </summary>
        /// <param name="quantidade">Quantidade de senhas a gerar</param>
        /// <param name="opcoes">Opções de configuração das senhas</param>
        /// <returns>Lista de senhas geradas</returns>
        IEnumerable<string> GerarSenhas(int quantidade, PasswordOptions? opcoes = null);

        /// <summary>
        /// Valida se uma senha atende aos critérios mínimos
        /// </summary>
        /// <param name="senha">Senha a validar</param>
        /// <returns>True se a senha é válida</returns>
        bool ValidarSenha(string senha);
    }

    /// <summary>
    /// Opções de configuração para geração de senhas
    /// </summary>
    public class PasswordOptions
    {
        /// <summary>
        /// Comprimento da senha (padrão: 12)
        /// </summary>
        public int Comprimento { get; set; } = 12;

        /// <summary>
        /// Incluir letras minúsculas (padrão: true)
        /// </summary>
        public bool IncluirMinusculas { get; set; } = true;

        /// <summary>
        /// Incluir letras maiúsculas (padrão: true)
        /// </summary>
        public bool IncluirMaiusculas { get; set; } = true;

        /// <summary>
        /// Incluir números (padrão: true)
        /// </summary>
        public bool IncluirNumeros { get; set; } = true;

        /// <summary>
        /// Incluir símbolos especiais (padrão: true)
        /// </summary>
        public bool IncluirSimbolos { get; set; } = true;

        /// <summary>
        /// Excluir caracteres ambíguos como 0, O, l, I (padrão: false)
        /// </summary>
        public bool ExcluirAmbiguos { get; set; } = false;

        /// <summary>
        /// Garantir pelo menos um caractere de cada tipo habilitado (padrão: true)
        /// </summary>
        public bool GarantirDiversidade { get; set; } = true;

        /// <summary>
        /// Símbolos personalizados a usar (se null, usa padrão)
        /// </summary>
        public string? SimbolosCustomizados { get; set; }
    }
}
