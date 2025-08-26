using Projeto.Moope.API.DTOs;
using Projeto.Moope.Core.DTOs.Vendas;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IVendaService
    {
        Task<Result<Pedido>> ProcessarVendaAsync(VendaStoreDto vendaDto);
        Task<VendaResponseDto> ConsultarVendaAsync(Guid vendaId);
        Task<IEnumerable<VendaResponseDto>> ListarVendasPorVendedorAsync(Guid vendedorId);
        Task<IEnumerable<VendaResponseDto>> ListarVendasPorClienteAsync(string email);
    }
}
