using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto.Moope.Core.DTOs.Clientes
{
    public class ClienteStoreDto
    {
        public string CpfCnpj { get; set; }
        public string Senha { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public string InscricaoEstadual { get; set; }
    }
}
