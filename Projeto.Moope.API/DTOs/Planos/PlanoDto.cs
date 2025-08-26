using Projeto.Moope.API.Utils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.API.DTOs.Planos
{
    public class PlanoDto
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "O campo Código é obrigatório")]
        public string Codigo { get; set; }
        [Required(ErrorMessage = "O campo Descrição é obrigatório")]
        public string Descricao { get; set; }
        [Required(ErrorMessage = "O campo Valor é obrigatório")]
        [DecimalRange(0.01, 99999999.99, ErrorMessage = "O Valor deve estar entre 0,01 e 99.999.999,99.")]
        public decimal Valor { get; set; }
        public bool Status { get; set; }
    }
}