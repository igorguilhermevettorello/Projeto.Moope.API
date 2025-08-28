using Projeto.Moope.API.Utils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.API.DTOs.Planos
{
    public class ListPlanoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public bool Status { get; set; }
    }
}
