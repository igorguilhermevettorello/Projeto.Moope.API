using Microsoft.Extensions.Options;
using Projeto.Moope.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Projeto.Moope.Core.Validation;

namespace Projeto.Moope.API.Attributes
{
    public class DocumentoAttribute : ValidationAttribute
    {
        private readonly string _tipoPessoaCampo;

        public DocumentoAttribute(string tipoPessoaCampo)
        {
            _tipoPessoaCampo = tipoPessoaCampo;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string documento = Convert.ToString(value)?.Trim().Replace(".", "").Replace("-", "").Replace("/", "");

            var tipoPessoaProp = validationContext.ObjectType.GetProperty(_tipoPessoaCampo);
            if (tipoPessoaProp == null)
                return new ValidationResult($"Campo '{_tipoPessoaCampo}' não encontrado.");

            var tipoPessoa = (TipoPessoa)tipoPessoaProp.GetValue(validationContext.ObjectInstance);
            if (tipoPessoa == null)
                return new ValidationResult("Tipo de pessoa não informado.");

            var valido = tipoPessoa == TipoPessoa.FISICA
                ? Documentos.IsValidCpf(documento)
                : Documentos.IsValidCnpj(documento);
            
            return (valido)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "Documento inválido.");
        }
    }
}
