using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Reembolso.Funcionarios.API.Data;
using Reembolso.Funcionarios.API.DTOs;
using Reembolso.Funcionarios.API.Models;
using Reembolso.Shared.DTOs;

namespace Reembolso.Funcionarios.API.Services;

public interface IColaboradorService
{
    Task<PagedResult<ColaboradorDto>> ObterTodosAsync(int pagina, int itensPorPagina, string? filtro = null);
    Task<ColaboradorDto?> ObterPorIdAsync(Guid id);
    Task<ColaboradorDto?> ObterPorMatriculaAsync(string matricula);
    Task<ColaboradorDto?> ObterPorEmailAsync(string email);
    Task<List<ColaboradorResumoDto>> ObterResumoTodosAsync();
    Task<List<ColaboradorResumoDto>> ObterAtivosAsync();
    Task<ColaboradorDto> CriarAsync(CriarColaboradorDto criarColaboradorDto);
    Task<ColaboradorDto?> AtualizarAsync(Guid id, AtualizarColaboradorDto atualizarColaboradorDto);
    Task<bool> ExcluirAsync(Guid id);
    Task<bool> ToggleAtivoAsync(Guid id);
    Task<bool> ExisteMatriculaAsync(string matricula, Guid? ignorarId = null);
    Task<bool> ExisteEmailAsync(string email, Guid? ignorarId = null);
    Task<bool> ExisteCPFAsync(string cpf, Guid? ignorarId = null);
}

public class ColaboradorService : IColaboradorService
{
    private readonly FuncionariosDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ColaboradorService> _logger;

    public ColaboradorService(
        FuncionariosDbContext context,
        IMapper mapper,
        ILogger<ColaboradorService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ColaboradorDto>> ObterTodosAsync(int pagina, int itensPorPagina, string? filtro = null)
    {
        var query = _context.Colaboradores.AsQueryable();

        // Aplicar filtro se fornecido
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(c => 
                c.Nome.Contains(filtro) ||
                c.Matricula.Contains(filtro) ||
                c.Email.Contains(filtro) ||
                (c.Cargo != null && c.Cargo.Contains(filtro)) ||
                (c.Departamento != null && c.Departamento.Contains(filtro)));
        }

        var totalItens = await query.CountAsync();
        
        var colaboradores = await query
            .OrderBy(c => c.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var colaboradoresDto = _mapper.Map<List<ColaboradorDto>>(colaboradores);

        return PagedResult<ColaboradorDto>.Create(colaboradoresDto, totalItens, pagina, itensPorPagina);
    }

    public async Task<ColaboradorDto?> ObterPorIdAsync(Guid id)
    {
        var colaborador = await _context.Colaboradores
            .FirstOrDefaultAsync(c => c.Id == id);

        return colaborador != null ? _mapper.Map<ColaboradorDto>(colaborador) : null;
    }

    public async Task<ColaboradorDto?> ObterPorMatriculaAsync(string matricula)
    {
        var colaborador = await _context.Colaboradores
            .FirstOrDefaultAsync(c => c.Matricula == matricula);

        return colaborador != null ? _mapper.Map<ColaboradorDto>(colaborador) : null;
    }

    public async Task<ColaboradorDto?> ObterPorEmailAsync(string email)
    {
        var colaborador = await _context.Colaboradores
            .FirstOrDefaultAsync(c => c.Email == email);

        return colaborador != null ? _mapper.Map<ColaboradorDto>(colaborador) : null;
    }

    public async Task<List<ColaboradorResumoDto>> ObterResumoTodosAsync()
    {
        var colaboradores = await _context.Colaboradores
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return _mapper.Map<List<ColaboradorResumoDto>>(colaboradores);
    }

    public async Task<List<ColaboradorResumoDto>> ObterAtivosAsync()
    {
        var colaboradores = await _context.Colaboradores
            .Where(c => c.Ativo && c.DataDemissao == null)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return _mapper.Map<List<ColaboradorResumoDto>>(colaboradores);
    }

    public async Task<ColaboradorDto> CriarAsync(CriarColaboradorDto criarColaboradorDto)
    {
        // Validações de negócio
        if (await ExisteMatriculaAsync(criarColaboradorDto.Matricula))
        {
            throw new ArgumentException("Já existe um colaborador com esta matrícula");
        }

        if (await ExisteEmailAsync(criarColaboradorDto.Email))
        {
            throw new ArgumentException("Já existe um colaborador com este email");
        }

        if (!string.IsNullOrEmpty(criarColaboradorDto.CPF) && await ExisteCPFAsync(criarColaboradorDto.CPF))
        {
            throw new ArgumentException("Já existe um colaborador com este CPF");
        }

        // Validar data de admissão
        if (criarColaboradorDto.DataAdmissao > DateTime.Now)
        {
            throw new ArgumentException("Data de admissão não pode ser futura");
        }

        // Validar data de nascimento
        if (criarColaboradorDto.DataNascimento > DateTime.Now.AddYears(-16))
        {
            throw new ArgumentException("Colaborador deve ter pelo menos 16 anos");
        }

        // Validar data de demissão
        if (criarColaboradorDto.DataDemissao.HasValue && criarColaboradorDto.DataDemissao < criarColaboradorDto.DataAdmissao)
        {
            throw new ArgumentException("Data de demissão não pode ser anterior à data de admissão");
        }

        var colaborador = _mapper.Map<Colaborador>(criarColaboradorDto);
        colaborador.Id = Guid.NewGuid();
        colaborador.CriadoPor = "Sistema"; // TODO: Obter do contexto do usuário

        _context.Colaboradores.Add(colaborador);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Colaborador criado: {ColaboradorId} - {Nome}", colaborador.Id, colaborador.Nome);

        return _mapper.Map<ColaboradorDto>(colaborador);
    }

    public async Task<ColaboradorDto?> AtualizarAsync(Guid id, AtualizarColaboradorDto atualizarColaboradorDto)
    {
        var colaborador = await _context.Colaboradores.FindAsync(id);
        if (colaborador == null)
        {
            return null;
        }

        // Validar data de demissão
        if (atualizarColaboradorDto.DataDemissao.HasValue && atualizarColaboradorDto.DataDemissao < colaborador.DataAdmissao)
        {
            throw new ArgumentException("Data de demissão não pode ser anterior à data de admissão");
        }

        // Mapear apenas os campos que podem ser atualizados
        _mapper.Map(atualizarColaboradorDto, colaborador);
        colaborador.AtualizadoPor = "Sistema"; // TODO: Obter do contexto do usuário

        await _context.SaveChangesAsync();

        _logger.LogInformation("Colaborador atualizado: {ColaboradorId} - {Nome}", colaborador.Id, colaborador.Nome);

        return _mapper.Map<ColaboradorDto>(colaborador);
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var colaborador = await _context.Colaboradores.FindAsync(id);
        if (colaborador == null)
        {
            return false;
        }

        // Soft delete
        colaborador.Ativo = false;
        colaborador.AtualizadoPor = "Sistema"; // TODO: Obter do contexto do usuário
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Colaborador excluído (soft delete): {ColaboradorId} - {Nome}", colaborador.Id, colaborador.Nome);

        return true;
    }

    public async Task<bool> ToggleAtivoAsync(Guid id)
    {
        var colaborador = await _context.Colaboradores.FindAsync(id);
        if (colaborador == null)
        {
            return false;
        }

        colaborador.Ativo = !colaborador.Ativo;
        colaborador.AtualizadoPor = "Sistema"; // TODO: Obter do contexto do usuário
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Status do colaborador alterado: {ColaboradorId} - {Nome} - Ativo: {Ativo}", 
            colaborador.Id, colaborador.Nome, colaborador.Ativo);

        return true;
    }

    public async Task<bool> ExisteMatriculaAsync(string matricula, Guid? ignorarId = null)
    {
        var query = _context.Colaboradores.Where(c => c.Matricula == matricula);
        
        if (ignorarId.HasValue)
        {
            query = query.Where(c => c.Id != ignorarId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExisteEmailAsync(string email, Guid? ignorarId = null)
    {
        var query = _context.Colaboradores.Where(c => c.Email == email);
        
        if (ignorarId.HasValue)
        {
            query = query.Where(c => c.Id != ignorarId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExisteCPFAsync(string cpf, Guid? ignorarId = null)
    {
        if (string.IsNullOrEmpty(cpf))
        {
            return false;
        }

        var query = _context.Colaboradores.Where(c => c.CPF == cpf);
        
        if (ignorarId.HasValue)
        {
            query = query.Where(c => c.Id != ignorarId.Value);
        }

        return await query.AnyAsync();
    }
}