namespace Projeto.Moope.Core.Models.Validators.Base
{
    public class Result<T>
    {
        public bool Status { get; set; }
        public string? Mensagem { get; set; }
        public T? Dados { get; set; }
    }
    
    public class Result
    {
        public bool Status { get; set; }
        public string? Mensagem { get; set; }
    }
    
    public class ResultUser<T>
    {
        public bool Status { get; set; }
        public string? Mensagem { get; set; }
        public T? Dados { get; set; }
        public bool UsuarioExiste { get; set; } =  false;
    }
}
