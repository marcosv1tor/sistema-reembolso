using AutoMapper;
using Reembolso.SolicitacoesReembolso.API.DTOs;
using Reembolso.SolicitacoesReembolso.API.Models;

namespace Reembolso.SolicitacoesReembolso.API.Mappings;

public class SolicitacaoReembolsoProfile : Profile
{
    public SolicitacaoReembolsoProfile()
    {
        // SolicitacaoReembolso -> DTOs
        CreateMap<SolicitacaoReembolso, SolicitacaoReembolsoDto>()
            .ForMember(dest => dest.QuantidadeAnexos, opt => opt.MapFrom(src => src.Anexos.Count(a => a.Ativo)))
            .ForMember(dest => dest.Anexos, opt => opt.MapFrom(src => src.Anexos.Where(a => a.Ativo)))
            .ForMember(dest => dest.HistoricoStatus, opt => opt.MapFrom(src => src.HistoricoStatus.OrderByDescending(h => h.DataMudanca)));

        CreateMap<SolicitacaoReembolso, SolicitacaoReembolsoResumoDto>()
            .ForMember(dest => dest.QuantidadeAnexos, opt => opt.MapFrom(src => src.Anexos.Count(a => a.Ativo)));

        // DTOs -> SolicitacaoReembolso
        CreateMap<CriarSolicitacaoReembolsoDto, SolicitacaoReembolso>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ValorAprovado, opt => opt.Ignore())
            .ForMember(dest => dest.DataAprovacao, opt => opt.Ignore())
            .ForMember(dest => dest.AprovadoPorId, opt => opt.Ignore())
            .ForMember(dest => dest.ObservacaoAprovacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataPagamento, opt => opt.Ignore())
            .ForMember(dest => dest.PagoPorId, opt => opt.Ignore())
            .ForMember(dest => dest.ObservacaoPagamento, opt => opt.Ignore())
            .ForMember(dest => dest.DataCancelamento, opt => opt.Ignore())
            .ForMember(dest => dest.MotivoCancelamento, opt => opt.Ignore())
            .ForMember(dest => dest.Anexos, opt => opt.Ignore())
            .ForMember(dest => dest.HistoricoStatus, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.AtualizadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.Ignore());

        CreateMap<AtualizarSolicitacaoReembolsoDto, SolicitacaoReembolso>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ColaboradorId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ValorAprovado, opt => opt.Ignore())
            .ForMember(dest => dest.DataAprovacao, opt => opt.Ignore())
            .ForMember(dest => dest.AprovadoPorId, opt => opt.Ignore())
            .ForMember(dest => dest.ObservacaoAprovacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataPagamento, opt => opt.Ignore())
            .ForMember(dest => dest.PagoPorId, opt => opt.Ignore())
            .ForMember(dest => dest.ObservacaoPagamento, opt => opt.Ignore())
            .ForMember(dest => dest.DataCancelamento, opt => opt.Ignore())
            .ForMember(dest => dest.MotivoCancelamento, opt => opt.Ignore())
            .ForMember(dest => dest.Anexos, opt => opt.Ignore())
            .ForMember(dest => dest.HistoricoStatus, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.AtualizadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.Ignore());

        // AnexoSolicitacao -> DTOs
        CreateMap<AnexoSolicitacao, AnexoSolicitacaoDto>();

        // HistoricoStatusSolicitacao -> DTOs
        CreateMap<HistoricoStatusSolicitacao, HistoricoStatusSolicitacaoDto>();
    }
}