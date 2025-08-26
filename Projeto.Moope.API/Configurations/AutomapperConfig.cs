using AutoMapper;
using Projeto.Moope.API.DTOs;
using Projeto.Moope.API.DTOs.Clientes;
using Projeto.Moope.API.DTOs.Enderecos;
using Projeto.Moope.API.DTOs.Planos;
using Projeto.Moope.API.DTOs.Revendedor;
using Projeto.Moope.API.DTOs.Vendas;
using Projeto.Moope.Core.Commands.Clientes.Atualizar;
using Projeto.Moope.Core.Commands.Clientes.Criar;
using Projeto.Moope.Core.Commands.Vendas;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.API.Configurations
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<Plano, PlanoDto>().ReverseMap();
            
            // Mapeamentos do Cliente
            // CreateMap<Cliente, ClienteDto>()
            //     .ForMember(dest => dest.PapelNome, opt => opt.MapFrom(src => src.Papel.Nome.ToString()));

            // cliente
            
            CreateMap<Cliente, ListClienteDto>().ReverseMap();
            
            CreateMap<CreateClienteDto, Cliente>();
            CreateMap<CreateClienteDto, Usuario>();
            CreateMap<CreateClienteDto, PessoaFisica>()
                .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.CpfCnpj));
            
            CreateMap<CreateClienteDto, PessoaJuridica>()
                .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.RazaoSocial, opt => opt.MapFrom(src => src.Nome));
            
            CreateMap<UpdateClienteDto, PessoaFisica>()
                .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.CpfCnpj));
            
            CreateMap<UpdateClienteDto, PessoaJuridica>()
                .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.RazaoSocial, opt => opt.MapFrom(src => src.Nome));
            
            CreateMap<UpdateClienteDto, Cliente>();
            CreateMap<UpdateClienteDto, Usuario>();
            CreateMap<Endereco, CreateEnderecoDto>().ReverseMap();
            CreateMap<UpdateEnderecoDto, Endereco>();

            CreateMap<Usuario, Usuario>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore());
            
            // vendedor
            
            CreateMap<CreateVendedorDto, Vendedor>().ReverseMap();
            CreateMap<CreateVendedorDto, Usuario>();
            CreateMap<CreateVendedorDto, PessoaFisica>()
                .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.CpfCnpj));
            
            CreateMap<CreateVendedorDto, PessoaJuridica>()
                .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.RazaoSocial, opt => opt.MapFrom(src => src.Nome));
            
            CreateMap<UpdateVendedorDto, Vendedor>();
            CreateMap<UpdateVendedorDto, Usuario>();
            CreateMap<UpdateVendedorDto, PessoaFisica>()
                .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.CpfCnpj));
            
            CreateMap<UpdateVendedorDto, PessoaJuridica>()
                .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.RazaoSocial, opt => opt.MapFrom(src => src.Nome));
            
            CreateMap<CreateVendaDto, VendaStoreDto>().ReverseMap();
            CreateMap<CreateVendaDto, ProcessarVendaCommand>()
                .ForMember(dest => dest.NomeCliente, opt => opt.MapFrom(src => src.NomeCliente))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
                .ForMember(dest => dest.NumeroCartao, opt => opt.MapFrom(src => src.NumeroCartao))
                .ForMember(dest => dest.Cvv, opt => opt.MapFrom(src => src.Cvv))
                .ForMember(dest => dest.DataValidade, opt => opt.MapFrom(src => src.DataValidade))
                .ForMember(dest => dest.VendedorId, opt => opt.MapFrom(src => src.VendedorId ?? Guid.Empty))
                .ForMember(dest => dest.PlanoId, opt => opt.MapFrom(src => src.PlanoId ?? Guid.Empty))
                .ForMember(dest => dest.Quantidade, opt => opt.MapFrom(src => src.Quantidade))
                .ForMember(dest => dest.Valor, opt => opt.Ignore()) // Será calculado no handler
                .ForMember(dest => dest.Descricao, opt => opt.Ignore())
                .ForMember(dest => dest.ClienteId, opt => opt.Ignore());

            CreateMap<CreateClienteDto, CriarClienteCommand>()
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CpfCnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
                .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => src.TipoPessoa))
                .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo))
                .ForMember(dest => dest.Senha, opt => opt.MapFrom(src => src.Senha))
                .ForMember(dest => dest.Confirmacao, opt => opt.MapFrom(src => src.Confirmacao))
                .ForMember(dest => dest.NomeFantasia, opt => opt.MapFrom(src => src.NomeFantasia))
                .ForMember(dest => dest.InscricaoEstadual, opt => opt.MapFrom(src => src.InscricaoEstadual))
                .ForMember(dest => dest.VendedorId, opt => opt.MapFrom(src => src.VendedorId))
                // Mapeamento do endereço (opcional)
                .ForMember(dest => dest.Logradouro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Logradouro : null))
                .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Numero : null))
                .ForMember(dest => dest.Complemento, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Complemento : null))
                .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Bairro : null))
                .ForMember(dest => dest.Cidade, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cidade : null))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Estado : null))
                .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cep : null));

            CreateMap<UpdateClienteDto, AtualizarClienteCommand>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CpfCnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
                .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => src.TipoPessoa))
                .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo))
                .ForMember(dest => dest.NomeFantasia, opt => opt.MapFrom(src => src.NomeFantasia))
                .ForMember(dest => dest.InscricaoEstadual, opt => opt.MapFrom(src => src.InscricaoEstadual))
                .ForMember(dest => dest.VendedorId, opt => opt.MapFrom(src => src.VendedorId))
                // Mapeamento do endereço
                //.ForMember(dest => dest.EnderecoId, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Id : null))
                .ForMember(dest => dest.Logradouro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Logradouro : null))
                .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Numero : null))
                .ForMember(dest => dest.Complemento, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Complemento : null))
                .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Bairro : null))
                .ForMember(dest => dest.Cidade, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cidade : null))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Estado : null))
                .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cep : null));

            // Mapeamento de CreateVendaDto para CriarClienteCommand (usado no processo de venda)
            CreateMap<CreateVendaDto, CriarClienteCommand>()
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.NomeCliente))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CpfCnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
                .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => src.TipoPessoa))
                .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.VendedorId, opt => opt.MapFrom(src => src.VendedorId))
                // Campos obrigatórios que não existem no CreateVendaDto - usar valores padrão
                .ForMember(dest => dest.Senha, opt => opt.MapFrom(src => "ClienteVenda123!")) // Senha temporária - deve ser alterada
                .ForMember(dest => dest.Confirmacao, opt => opt.MapFrom(src => "ClienteVenda123!")) // Confirmação da senha temporária
                .ForMember(dest => dest.NomeFantasia, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.InscricaoEstadual, opt => opt.MapFrom(src => string.Empty))
                // Endereço não informado na venda
                .ForMember(dest => dest.Logradouro, opt => opt.MapFrom(src => (string)null))
                .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => (string)null))
                .ForMember(dest => dest.Complemento, opt => opt.MapFrom(src => (string)null))
                .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => (string)null))
                .ForMember(dest => dest.Cidade, opt => opt.MapFrom(src => (string)null))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (string)null))
                .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => (string)null));
        }
    }
}
