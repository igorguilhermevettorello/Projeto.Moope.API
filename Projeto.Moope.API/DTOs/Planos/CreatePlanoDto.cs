using Projeto.Moope.API.Utils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.API.DTOs.Planos
{
    public class CreatePlanoDto
    {
        [Required(ErrorMessage = "O campo C�digo � obrigat�rio")]
        public string Codigo { get; set; }
        [Required(ErrorMessage = "O campo Descri��o � obrigat�rio")]
        public string Descricao { get; set; }
        [Required(ErrorMessage = "O campo Valor � obrigat�rio")]
        [DecimalRange(0.01, 99999999.99, ErrorMessage = "O Valor deve estar entre 0,01 e 99.999.999,99.")]
        public decimal Valor { get; set; }
        public bool Status { get; set; }
    }
}
