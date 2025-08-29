namespace Projeto.Moope.API.DTOs.Planos
{
    public class DetailPlanoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public bool Status { get; set; }
    }    
}

