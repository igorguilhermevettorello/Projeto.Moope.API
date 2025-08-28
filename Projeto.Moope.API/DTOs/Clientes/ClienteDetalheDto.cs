namespace Projeto.Moope.API.DTOs.Clientes
{
    public class ClienteDetalheDto
    {
        public Guid Id { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public string? TipoPessoa { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Telefone { get; set; }
        public bool Ativo { get; set; }
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
    }
}
