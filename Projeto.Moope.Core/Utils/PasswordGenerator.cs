using System.Security.Cryptography;
using System.Text;
using Projeto.Moope.Core.Interfaces.Utils;

namespace Projeto.Moope.Core.Utils
{
    /// <summary>
    /// Implementação para geração segura de senhas automáticas
    /// </summary>
    public class PasswordGenerator : IPasswordGenerator
    {
        private const string MINUSCULAS = "abcdefghijklmnopqrstuvwxyz";
        private const string MAIUSCULAS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NUMEROS = "0123456789";
        private const string SIMBOLOS_PADRAO = "!@#$%&*+-=?";
        private const string CARACTERES_AMBIGUOS = "0O1lI";

        private readonly RandomNumberGenerator _rng;

        public PasswordGenerator()
        {
            _rng = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// Gera uma senha aleatória com configurações padrão (12 caracteres)
        /// </summary>
        public string GerarSenha()
        {
            return GerarSenha(new PasswordOptions());
        }

        /// <summary>
        /// Gera uma senha aleatória com comprimento específico
        /// </summary>
        public string GerarSenha(int comprimento)
        {
            if (comprimento < 6)
                throw new ArgumentException("Comprimento mínimo da senha é 6 caracteres", nameof(comprimento));

            if (comprimento > 128)
                throw new ArgumentException("Comprimento máximo da senha é 128 caracteres", nameof(comprimento));

            return GerarSenha(new PasswordOptions { Comprimento = comprimento });
        }

        /// <summary>
        /// Gera uma senha aleatória com configurações personalizadas
        /// </summary>
        public string GerarSenha(PasswordOptions opcoes)
        {
            if (opcoes == null)
                throw new ArgumentNullException(nameof(opcoes));

            ValidarOpcoes(opcoes);

            var caracteresDisponiveis = ConstruirConjuntoCaracteres(opcoes);
            var senha = new StringBuilder();

            // Se garantir diversidade está habilitado, adicionar pelo menos um caractere de cada tipo
            if (opcoes.GarantirDiversidade)
            {
                AdicionarCaracteresObrigatorios(senha, opcoes);
            }

            // Preencher o restante da senha aleatoriamente
            var caracteresRestantes = opcoes.Comprimento - senha.Length;
            for (int i = 0; i < caracteresRestantes; i++)
            {
                var caractereSelecionado = SelecionarCaractereAleatorio(caracteresDisponiveis);
                senha.Append(caractereSelecionado);
            }

            // Embaralhar a senha para evitar padrões previsíveis
            return EmbaralharString(senha.ToString());
        }

        /// <summary>
        /// Gera múltiplas senhas aleatórias
        /// </summary>
        public IEnumerable<string> GerarSenhas(int quantidade, PasswordOptions? opcoes = null)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));

            if (quantidade > 1000)
                throw new ArgumentException("Quantidade máxima é 1000 senhas por vez", nameof(quantidade));

            opcoes ??= new PasswordOptions();
            var senhas = new List<string>();

            for (int i = 0; i < quantidade; i++)
            {
                senhas.Add(GerarSenha(opcoes));
            }

            return senhas;
        }

        /// <summary>
        /// Valida se uma senha atende aos critérios mínimos
        /// </summary>
        public bool ValidarSenha(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                return false;

            if (senha.Length < 6)
                return false;

            var temMinuscula = senha.Any(char.IsLower);
            var temMaiuscula = senha.Any(char.IsUpper);
            var temNumero = senha.Any(char.IsDigit);
            var temSimbolo = senha.Any(c => SIMBOLOS_PADRAO.Contains(c));

            // Pelo menos 3 dos 4 tipos de caracteres
            var tiposPresentes = new[] { temMinuscula, temMaiuscula, temNumero, temSimbolo }.Count(x => x);
            return tiposPresentes >= 3;
        }

        #region Métodos Privados

        private void ValidarOpcoes(PasswordOptions opcoes)
        {
            if (opcoes.Comprimento < 6)
                throw new ArgumentException("Comprimento mínimo da senha é 6 caracteres");

            if (opcoes.Comprimento > 128)
                throw new ArgumentException("Comprimento máximo da senha é 128 caracteres");

            if (!opcoes.IncluirMinusculas && !opcoes.IncluirMaiusculas && 
                !opcoes.IncluirNumeros && !opcoes.IncluirSimbolos)
                throw new ArgumentException("Pelo menos um tipo de caractere deve ser habilitado");

            if (opcoes.GarantirDiversidade)
            {
                var tiposHabilitados = new[] 
                { 
                    opcoes.IncluirMinusculas, 
                    opcoes.IncluirMaiusculas, 
                    opcoes.IncluirNumeros, 
                    opcoes.IncluirSimbolos 
                }.Count(x => x);

                if (opcoes.Comprimento < tiposHabilitados)
                    throw new ArgumentException("Comprimento insuficiente para garantir diversidade de caracteres");
            }
        }

        private string ConstruirConjuntoCaracteres(PasswordOptions opcoes)
        {
            var conjunto = new StringBuilder();

            if (opcoes.IncluirMinusculas)
            {
                var minusculas = opcoes.ExcluirAmbiguos 
                    ? RemoverCaracteresAmbiguos(MINUSCULAS) 
                    : MINUSCULAS;
                conjunto.Append(minusculas);
            }

            if (opcoes.IncluirMaiusculas)
            {
                var maiusculas = opcoes.ExcluirAmbiguos 
                    ? RemoverCaracteresAmbiguos(MAIUSCULAS) 
                    : MAIUSCULAS;
                conjunto.Append(maiusculas);
            }

            if (opcoes.IncluirNumeros)
            {
                var numeros = opcoes.ExcluirAmbiguos 
                    ? RemoverCaracteresAmbiguos(NUMEROS) 
                    : NUMEROS;
                conjunto.Append(numeros);
            }

            if (opcoes.IncluirSimbolos)
            {
                var simbolos = !string.IsNullOrEmpty(opcoes.SimbolosCustomizados) 
                    ? opcoes.SimbolosCustomizados 
                    : SIMBOLOS_PADRAO;
                conjunto.Append(simbolos);
            }

            return conjunto.ToString();
        }

        private void AdicionarCaracteresObrigatorios(StringBuilder senha, PasswordOptions opcoes)
        {
            if (opcoes.IncluirMinusculas)
            {
                var minusculas = opcoes.ExcluirAmbiguos 
                    ? RemoverCaracteresAmbiguos(MINUSCULAS) 
                    : MINUSCULAS;
                senha.Append(SelecionarCaractereAleatorio(minusculas));
            }

            if (opcoes.IncluirMaiusculas)
            {
                var maiusculas = opcoes.ExcluirAmbiguos 
                    ? RemoverCaracteresAmbiguos(MAIUSCULAS) 
                    : MAIUSCULAS;
                senha.Append(SelecionarCaractereAleatorio(maiusculas));
            }

            if (opcoes.IncluirNumeros)
            {
                var numeros = opcoes.ExcluirAmbiguos 
                    ? RemoverCaracteresAmbiguos(NUMEROS) 
                    : NUMEROS;
                senha.Append(SelecionarCaractereAleatorio(numeros));
            }

            if (opcoes.IncluirSimbolos)
            {
                var simbolos = !string.IsNullOrEmpty(opcoes.SimbolosCustomizados) 
                    ? opcoes.SimbolosCustomizados 
                    : SIMBOLOS_PADRAO;
                senha.Append(SelecionarCaractereAleatorio(simbolos));
            }
        }

        private string RemoverCaracteresAmbiguos(string caracteres)
        {
            return new string(caracteres.Where(c => !CARACTERES_AMBIGUOS.Contains(c)).ToArray());
        }

        private char SelecionarCaractereAleatorio(string caracteres)
        {
            if (string.IsNullOrEmpty(caracteres))
                throw new ArgumentException("Conjunto de caracteres não pode estar vazio");

            var bytes = new byte[4];
            _rng.GetBytes(bytes);
            var indice = Math.Abs(BitConverter.ToInt32(bytes, 0)) % caracteres.Length;
            return caracteres[indice];
        }

        private string EmbaralharString(string input)
        {
            var array = input.ToCharArray();
            
            for (int i = array.Length - 1; i > 0; i--)
            {
                var bytes = new byte[4];
                _rng.GetBytes(bytes);
                var j = Math.Abs(BitConverter.ToInt32(bytes, 0)) % (i + 1);
                
                // Trocar elementos
                (array[i], array[j]) = (array[j], array[i]);
            }
            
            return new string(array);
        }

        #endregion

        public void Dispose()
        {
            _rng?.Dispose();
        }
    }
}
