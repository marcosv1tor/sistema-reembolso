using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Reembolso.Shared.DTOs;
using Reembolso.Usuarios.API.Data;
using Reembolso.Usuarios.API.DTOs;
using Reembolso.Usuarios.API.Models;

namespace Reembolso.Usuarios.API.Services;

public interface IUsuarioService
{
    Task<PagedResult<UsuarioDto>> ObterTodosAsync(int pagina, int itensPorPagina, string? filtro);
    Task<UsuarioDto?> ObterPorIdAsync(Guid id);
    Task<UsuarioDto?> ObterPorEmailAsync(string email);
    Task<UsuarioDto> CriarAsync(CriarUsuarioDto criarUsuarioDto);
    Task<UsuarioDto?> AtualizarAsync(Guid id, AtualizarUsuarioDto atualizarUsuarioDto);
    Task<bool> ExcluirAsync(Guid id);
    Task<bool> ToggleAtivoAsync(Guid id);
}

public class UsuarioService : IUsuarioService
{
    private readonly UsuariosDbContext _context;
    private readonly IMapper _mapper;

    public UsuarioService(UsuariosDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<UsuarioDto>> ObterTodosAsync(int pagina, int itensPorPagina, string? filtro)
    {
        var query = _context.Usuarios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(u => u.Nome.Contains(filtro) || u.Email.Contains(filtro));
        }

        var totalItems = await query.CountAsync();
        
        var usuarios = await query
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var usuariosDto = _mapper.Map<List<UsuarioDto>>(usuarios);
        
        return PagedResult<UsuarioDto>.Create(usuariosDto, totalItems, pagina, itensPorPagina);
    }

    public async Task<UsuarioDto?> ObterPorIdAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        return usuario == null ? null : _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto?> ObterPorEmailAsync(string email)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);
        return usuario == null ? null : _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto> CriarAsync(CriarUsuarioDto criarUsuarioDto)
    {
        // Verificar se email já existe
        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == criarUsuarioDto.Email);
        
        if (emailExiste)
            throw new ArgumentException("Email já está em uso");

        var usuario = new Usuario
        {
            Nome = criarUsuarioDto.Nome,
            Email = criarUsuarioDto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(criarUsuarioDto.Senha),
            TipoUsuario = criarUsuarioDto.TipoUsuario,
            DataCriacao = DateTime.UtcNow,
            Ativo = true,
            PrimeiroLogin = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto?> AtualizarAsync(Guid id, AtualizarUsuarioDto atualizarUsuarioDto)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return null;

        // Verificar se email já existe (exceto para o próprio usuário)
        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == atualizarUsuarioDto.Email && u.Id != id);
        
        if (emailExiste)
            throw new ArgumentException("Email já está em uso");

        usuario.Nome = atualizarUsuarioDto.Nome;
        usuario.Email = atualizarUsuarioDto.Email;
        usuario.TipoUsuario = atualizarUsuarioDto.TipoUsuario;
        usuario.Ativo = atualizarUsuarioDto.Ativo;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return false;

        // Soft delete - apenas desativa o usuário
        usuario.Ativo = false;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleAtivoAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return false;

        usuario.Ativo = !usuario.Ativo;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}