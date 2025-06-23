using AutoMapper;
using Reembolso.Usuarios.API.DTOs;
using Reembolso.Usuarios.API.Models;

namespace Reembolso.Usuarios.API.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.TipoUsuarioDescricao, 
                       opt => opt.MapFrom(src => src.TipoUsuario.ToString()));

        CreateMap<CriarUsuarioDto, Usuario>()
            .ForMember(dest => dest.SenhaHash, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.Ignore())
            .ForMember(dest => dest.PrimeiroLogin, opt => opt.Ignore())
            .ForMember(dest => dest.UltimoLogin, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokenExpiry, opt => opt.Ignore())
            .ForMember(dest => dest.HistoricoLogins, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.AtualizadoPor, opt => opt.Ignore());

        CreateMap<AtualizarUsuarioDto, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SenhaHash, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.PrimeiroLogin, opt => opt.Ignore())
            .ForMember(dest => dest.UltimoLogin, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokenExpiry, opt => opt.Ignore())
            .ForMember(dest => dest.HistoricoLogins, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.AtualizadoPor, opt => opt.Ignore());
    }
}