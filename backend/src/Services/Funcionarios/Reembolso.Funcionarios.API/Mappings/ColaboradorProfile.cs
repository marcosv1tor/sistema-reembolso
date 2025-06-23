using AutoMapper;
using Reembolso.Funcionarios.API.DTOs;
using Reembolso.Funcionarios.API.Models;

namespace Reembolso.Funcionarios.API.Mappings;

public class ColaboradorProfile : Profile
{
    public ColaboradorProfile()
    {
        // Colaborador -> ColaboradorDto
        CreateMap<Colaborador, ColaboradorDto>()
            .ForMember(dest => dest.EstaAtivo, opt => opt.MapFrom(src => src.EstaAtivo))
            .ForMember(dest => dest.TempoEmpresaAnos, opt => opt.MapFrom(src => src.TempoEmpresaAnos));

        // CriarColaboradorDto -> Colaborador
        CreateMap<CriarColaboradorDto, Colaborador>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CriadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.AtualizadoPor, opt => opt.Ignore());

        // AtualizarColaboradorDto -> Colaborador
        CreateMap<AtualizarColaboradorDto, Colaborador>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Matricula, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.DataAdmissao, opt => opt.Ignore())
            .ForMember(dest => dest.CPF, opt => opt.Ignore())
            .ForMember(dest => dest.DataNascimento, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.AtualizadoPor, opt => opt.Ignore());

        // Colaborador -> ColaboradorResumoDto
        CreateMap<Colaborador, ColaboradorResumoDto>()
            .ForMember(dest => dest.EstaAtivo, opt => opt.MapFrom(src => src.EstaAtivo));
    }
}