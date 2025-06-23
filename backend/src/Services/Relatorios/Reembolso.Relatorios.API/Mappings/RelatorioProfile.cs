using AutoMapper;
using Reembolso.Relatorios.API.DTOs;
using Reembolso.Relatorios.API.Models;

namespace Reembolso.Relatorios.API.Mappings;

public class RelatorioProfile : Profile
{
    public RelatorioProfile()
    {
        // Mapeamento de Relatorio para RelatorioDto
        CreateMap<Relatorio, RelatorioDto>()
            .ForMember(dest => dest.EstaProcessando, opt => opt.MapFrom(src => src.EstaProcessando))
            .ForMember(dest => dest.EstaConcluido, opt => opt.MapFrom(src => src.EstaConcluido))
            .ForMember(dest => dest.TemErro, opt => opt.MapFrom(src => src.TemErro))
            .ForMember(dest => dest.PodeSerBaixado, opt => opt.MapFrom(src => src.PodeSerBaixado))
            .ForMember(dest => dest.TempoProcessamento, opt => opt.MapFrom(src => src.TempoProcessamento))
            .ForMember(dest => dest.TamanhoFormatado, opt => opt.MapFrom(src => src.TamanhoFormatado))
            .ForMember(dest => dest.DiasRetencao, opt => opt.MapFrom(src => src.DiasRetencao))
            .ForMember(dest => dest.DeveSerExcluido, opt => opt.MapFrom(src => src.DeveSerExcluido));

        // Mapeamento de Relatorio para RelatorioResumoDto
        CreateMap<Relatorio, RelatorioResumoDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.PodeSerBaixado, opt => opt.MapFrom(src => src.PodeSerBaixado))
            .ForMember(dest => dest.TamanhoFormatado, opt => opt.MapFrom(src => src.TamanhoFormatado))
            .ForMember(dest => dest.TempoProcessamento, opt => opt.MapFrom(src => src.TempoProcessamento));

        // Mapeamento de CriarRelatorioDto para Relatorio
        CreateMap<CriarRelatorioDto, Relatorio>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.DataGeracao, opt => opt.Ignore())
            .ForMember(dest => dest.DataConclusao, opt => opt.Ignore())
            .ForMember(dest => dest.CaminhoArquivo, opt => opt.Ignore())
            .ForMember(dest => dest.NomeArquivo, opt => opt.Ignore())
            .ForMember(dest => dest.TamanhoBytes, opt => opt.Ignore())
            .ForMember(dest => dest.TipoConteudo, opt => opt.Ignore())
            .ForMember(dest => dest.MensagemErro, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRegistros, opt => opt.Ignore())
            .ForMember(dest => dest.ValorTotal, opt => opt.Ignore())
            .ForMember(dest => dest.GeradoPorId, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.AtualizadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true));

        // Mapeamento de RelatorioDto para Relatorio (para atualizações)
        CreateMap<RelatorioDto, Relatorio>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.GeradoPorId, opt => opt.Ignore())
            .ForMember(dest => dest.DataGeracao, opt => opt.Ignore())
            .ForMember(dest => dest.DataConclusao, opt => opt.Ignore())
            .ForMember(dest => dest.CaminhoArquivo, opt => opt.Ignore())
            .ForMember(dest => dest.NomeArquivo, opt => opt.Ignore())
            .ForMember(dest => dest.TamanhoBytes, opt => opt.Ignore())
            .ForMember(dest => dest.TipoConteudo, opt => opt.Ignore())
            .ForMember(dest => dest.MensagemErro, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRegistros, opt => opt.Ignore())
            .ForMember(dest => dest.ValorTotal, opt => opt.Ignore())
            .ForMember(dest => dest.PodeSerBaixado, opt => opt.Ignore())
            .ForMember(dest => dest.TempoProcessamento, opt => opt.Ignore())
            .ForMember(dest => dest.TamanhoFormatado, opt => opt.Ignore())
            .ForMember(dest => dest.DiasRetencao, opt => opt.Ignore())
            .ForMember(dest => dest.DeveSerExcluido, opt => opt.Ignore());
    }
}