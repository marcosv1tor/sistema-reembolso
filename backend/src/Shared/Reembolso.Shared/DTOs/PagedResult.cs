namespace Reembolso.Shared.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int PaginaAtual { get; set; }
    public int ItensPorPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItems / ItensPorPagina);
    public bool TemPaginaAnterior => PaginaAtual > 1;
    public bool TemProximaPagina => PaginaAtual < TotalPaginas;
    
    // Aliases para compatibilidade
    public int CurrentPage => PaginaAtual;
    public int ItemsPerPage => ItensPorPagina;

    public PagedResult(List<T> items, int totalItems, int paginaAtual, int itensPorPagina)
    {
        Items = items;
        TotalItems = totalItems;
        PaginaAtual = paginaAtual;
        ItensPorPagina = itensPorPagina;
    }

    public static PagedResult<T> Create(List<T> items, int totalItems, int paginaAtual, int itensPorPagina)
    {
        return new PagedResult<T>(items, totalItems, paginaAtual, itensPorPagina);
    }
}