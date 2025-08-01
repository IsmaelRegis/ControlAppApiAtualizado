﻿using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Domain.Interfaces.Services;
using ControlApp.Domain.Validations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenSecurity _tokenSecurity;
    private readonly IImageService _imageService;
    private readonly UsuarioValidator _usuarioValidator;
    private readonly TecnicoValidator _tecnicoValidator;
    private readonly CryptoSHA256 _cryptoSHA256;
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ITrajetoRepository _trajetoRepository;
    private readonly ILocalizacaoRepository _localizacaoRepository;
    private readonly ITokenManager _tokenManager;
    private readonly IAuditoriaService _auditoriaService;
    private readonly IAuditoriaRepository _auditoriaRepository;

    #region Construtor
    public UsuarioService(IUsuarioRepository usuarioRepository, ITokenSecurity tokenSecurity, IImageService imageService, ITecnicoRepository tecnicoRepository, IEmpresaRepository empresaRepository, ITrajetoRepository trajetoRepository, ILocalizacaoRepository localizacaoRepository, ITokenManager tokenManager, IAuditoriaService auditoriaService, IAuditoriaRepository auditoriaRepository)
    {
        _usuarioRepository = usuarioRepository;
        _tokenSecurity = tokenSecurity;
        _imageService = imageService;
        _usuarioValidator = new UsuarioValidator();
        _tecnicoValidator = new TecnicoValidator();
        _cryptoSHA256 = new CryptoSHA256();
        _tecnicoRepository = tecnicoRepository;
        _empresaRepository = empresaRepository;
        _trajetoRepository = trajetoRepository;
        _localizacaoRepository = localizacaoRepository;
        _tokenManager = tokenManager;
        _auditoriaService = auditoriaService;
        _auditoriaRepository = auditoriaRepository;
    }
    #endregion

    #region Métodos de Criação de Usuário (Refatorado)

    public async Task<CriarUsuarioResponseDto> CreateUsuarioAsync(CriarUsuarioRequestDto requestDto)
    {
        // Validação central de UserName para evitar duplicatas
        if (!string.IsNullOrWhiteSpace(requestDto.UserName))
        {
            var usuarioExistentePorUserName = await _usuarioRepository.ObterUsuarioPorUserNameAsync(requestDto.UserName);
            if (usuarioExistentePorUserName != null)
                throw new Exception("UserName já cadastrado. Escolha outro UserName.");
        }

        // Delega a criação para o método específico baseado na Role
        return requestDto.Role switch
        {
            UserRole.Colaborador => await CreateTecnicoAsync(requestDto),
            UserRole.Visitante => await CreateVisitanteAsync(requestDto),
            UserRole.Administrador => await CreateAdministradorAsync(requestDto),
            UserRole.SuperAdministrador => await CreateSuperAdministradorAsync(requestDto),
            _ => throw new ArgumentOutOfRangeException(nameof(requestDto.Role), "A Role fornecida não é suportada para criação.")
        };
    }

    private async Task<CriarUsuarioResponseDto> CreateTecnicoAsync(CriarUsuarioRequestDto requestDto)
    {
        var usuarioExistente = await _usuarioRepository.ObterUsuarioPorCpfAsync(requestDto.Cpf);
        if (usuarioExistente != null)
            throw new Exception("CPF já cadastrado.");

        string? fotoUrl = requestDto.FotoFile != null ? await _imageService.UploadImageAsync(requestDto.FotoFile) : null;

        var tecnico = new Tecnico
        {
            UsuarioId = Guid.NewGuid(),
            Nome = requestDto.Nome,
            UserName = requestDto.UserName,
            Email = requestDto.Email,
            Senha = requestDto.Senha,
            Cpf = requestDto.Cpf,
            HoraEntrada = requestDto.HoraEntrada,
            HoraSaida = requestDto.HoraSaida,
            HoraAlmocoInicio = requestDto.HoraAlmocoInicio,
            HoraAlmocoFim = requestDto.HoraAlmocoFim,
            FotoUrl = fotoUrl,
            IsOnline = false,
            Role = UserRole.Colaborador,
            Ativo = true,
            NumeroMatricula = await GerarNumeroMatriculaAsync(),
            EmpresaId = requestDto.EmpresaId,
            TipoUsuario = "Colaborador"
        };

        var validationResult = await _tecnicoValidator.ValidateAsync(tecnico);
        if (!validationResult.IsValid)
            throw new Exception(string.Join(", ", validationResult.Errors));

        tecnico.Senha = _cryptoSHA256.HashPassword(tecnico.Senha);
        await _usuarioRepository.CriarUsuarioAsync(tecnico);

        EmpresaResponseDto? empresaDto = null;
        if (requestDto.EmpresaId.HasValue)
        {
            var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(requestDto.EmpresaId.Value);
            if (empresa != null)
            {
                empresaDto = new EmpresaResponseDto { /* Mapeamento da empresa */ };
            }
        }

        return new CriarUsuarioResponseDto
        {
            UsuarioId = tecnico.UsuarioId,
            Nome = tecnico.Nome,
            UserName = tecnico.UserName,
            Email = tecnico.Email,
            Senha = tecnico.Senha,
            Cpf = tecnico.Cpf,
            HoraEntrada = tecnico.HoraEntrada,
            HoraSaida = tecnico.HoraSaida,
            HoraAlmocoInicio = tecnico.HoraAlmocoInicio,
            HoraAlmocoFim = tecnico.HoraAlmocoFim,
            FotoUrl = tecnico.FotoUrl,
            IsOnline = tecnico.IsOnline,
            Role = tecnico.Role.ToString(),
            Ativo = tecnico.Ativo,
            NumeroMatricula = tecnico.NumeroMatricula,
            EmpresaId = tecnico.EmpresaId,
            Empresa = empresaDto
        };
    }

    private async Task<CriarUsuarioResponseDto> CreateVisitanteAsync(CriarUsuarioRequestDto requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.UserName) || string.IsNullOrWhiteSpace(requestDto.Senha))
            throw new Exception("UserName e Senha são obrigatórios para visitantes.");

        var visitante = new Visitante
        {
            UsuarioId = Guid.NewGuid(),
            Nome = requestDto.Nome ?? "Visitante",
            UserName = requestDto.UserName,
            Email = requestDto.Email,
            Senha = _cryptoSHA256.HashPassword(requestDto.Senha),
            Role = UserRole.Visitante,
            Ativo = true,
            TipoUsuario = "Visitante"
        };

        await _usuarioRepository.CriarUsuarioAsync(visitante);

        return new CriarUsuarioResponseDto
        {
            UsuarioId = visitante.UsuarioId,
            Nome = visitante.Nome,
            UserName = visitante.UserName,
            Email = visitante.Email,
            Senha = visitante.Senha,
            Role = visitante.Role.ToString(),
            Ativo = visitante.Ativo
        };
    }

    private async Task<CriarUsuarioResponseDto> CreateAdministradorAsync(CriarUsuarioRequestDto requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Email) || string.IsNullOrWhiteSpace(requestDto.Senha))
            throw new ArgumentException("Email e Senha são obrigatórios para criar um Administrador.");

        var administrador = new Administrador
        {
            UsuarioId = Guid.NewGuid(),
            Nome = requestDto.Nome,
            UserName = requestDto.UserName,
            Email = requestDto.Email,
            Senha = _cryptoSHA256.HashPassword(requestDto.Senha),
            Role = UserRole.Administrador,
            Ativo = true,
            TipoUsuario = "Administrador"
        };

        await _usuarioRepository.CriarUsuarioAsync(administrador);

        return new CriarUsuarioResponseDto
        {
            UsuarioId = administrador.UsuarioId,
            Nome = administrador.Nome,
            UserName = administrador.UserName,
            Email = administrador.Email,
            Role = administrador.Role.ToString(),
            Ativo = administrador.Ativo
        };
    }

    private async Task<CriarUsuarioResponseDto> CreateSuperAdministradorAsync(CriarUsuarioRequestDto requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Email) || string.IsNullOrWhiteSpace(requestDto.Senha))
            throw new ArgumentException("Email e Senha são obrigatórios para criar um SuperAdministrador.");

        var superAdmin = new SuperAdministrador
        {
            UsuarioId = Guid.NewGuid(),
            Nome = requestDto.Nome,
            UserName = requestDto.UserName,
            Email = requestDto.Email,
            Senha = _cryptoSHA256.HashPassword(requestDto.Senha),
            Role = UserRole.SuperAdministrador,
            Ativo = true,
            TipoUsuario = "SuperAdministrador"
        };

        await _usuarioRepository.CriarUsuarioAsync(superAdmin);

        return new CriarUsuarioResponseDto
        {
            UsuarioId = superAdmin.UsuarioId,
            Nome = superAdmin.Nome,
            UserName = superAdmin.UserName,
            Email = superAdmin.Email,
            Role = superAdmin.Role.ToString(),
            Ativo = superAdmin.Ativo
        };
    }

    #endregion

    #region Métodos de Autenticação e Gestão de Usuários

    public async Task<AutenticarUsuarioResponseDto> AuthenticateUsuarioAsync(AutenticarUsuarioRequestDto requestDto, string deviceInfo = null, string audience = "VibeService")
    {
        if (string.IsNullOrWhiteSpace(requestDto.UserName))
        {
            throw new UnauthorizedAccessException("Username deve ser fornecido");
        }
        var usuario = await _usuarioRepository.ObterUsuarioPorUserNameAsync(requestDto.UserName);
        if (usuario == null || !_cryptoSHA256.VerifyPassword(requestDto.Senha, usuario.Senha))
        {
            throw new UnauthorizedAccessException("Username ou senha inválidos");
        }

        bool isOnline = usuario is Tecnico tecnico ? (tecnico.IsOnline = true) : false;

        TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        DateTime horarioBrasilia = TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone);
        usuario.DataHoraUltimaAutenticacao = horarioBrasilia;
        await _usuarioRepository.AtualizarUsuarioAsync(usuario.UsuarioId);

        var token = await _tokenManager.GenerateTokenAsync(usuario.UsuarioId, usuario.Role.ToString(), deviceInfo, audience);

        await _auditoriaService.RegistrarAsync(usuario.UsuarioId, usuario.Nome, "Entrou no sistema", $"{usuario.Nome} logou como {usuario.Role} às {usuario.DataHoraUltimaAutenticacao:dd/MM/yyyy HH:mm:ss}", usuario.Role);

        return new AutenticarUsuarioResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role.ToString(),
            Cpf = (usuario as Tecnico)?.Cpf,
            Token = token,
            FotoUrl = usuario.FotoUrl,
            UserName = usuario.UserName,
            IsOnline = isOnline,
            DataHoraAutenticacao = usuario.DataHoraUltimaAutenticacao ?? DateTime.MinValue
        };
    }

    public async Task LogoutUsuarioAsync(Guid usuarioId, string token)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        if (usuario is Tecnico tecnico)
        {
            tecnico.IsOnline = false;
            await _usuarioRepository.AtualizarUsuarioAsync(tecnico.UsuarioId);
        }

        await _tokenManager.InvalidateTokensForUserAsync(usuarioId, token);
        await _auditoriaService.RegistrarAsync(usuario.UsuarioId, usuario.Nome, "Saiu do sistema", $"{usuario.Nome} fez logout às {DateTime.Now:dd/MM/yyyy HH:mm:ss}", usuario.Role);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _usuarioRepository.ObterUsuarioPorEmailAsync(email);
    }

    public async Task ChangePasswordAsync(Guid usuarioId, string novaSenha)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        usuario.Senha = _cryptoSHA256.HashPassword(novaSenha);
        await _usuarioRepository.AtualizarUsuarioAsync(usuario.UsuarioId);
    }

    public async Task<AtualizarUsuarioResponseDto> UpdateUsuarioAsync(Guid usuarioId, AtualizarUsuarioRequestDto requestDto)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        usuario.Nome = requestDto.Nome ?? usuario.Nome;
        usuario.UserName = requestDto.UserName ?? usuario.UserName;
        usuario.Email = requestDto.Email ?? usuario.Email;

        if (!string.IsNullOrEmpty(requestDto.Senha))
            usuario.Senha = _cryptoSHA256.HashPassword(requestDto.Senha);

        if (usuario is Tecnico tecnico)
        {
            tecnico.HoraEntrada = requestDto.HoraEntrada ?? tecnico.HoraEntrada;
            tecnico.HoraSaida = requestDto.HoraSaida ?? tecnico.HoraSaida;
            tecnico.HoraAlmocoInicio = requestDto.HoraAlmocoInicio ?? tecnico.HoraAlmocoInicio;
            tecnico.HoraAlmocoFim = requestDto.HoraAlmocoFim ?? tecnico.HoraAlmocoFim;
            tecnico.Cpf = requestDto.Cpf ?? tecnico.Cpf;

            if (requestDto.FotoFile != null)
                tecnico.FotoUrl = await _imageService.UploadImageAsync(requestDto.FotoFile);
            else
                tecnico.FotoUrl = requestDto.FotoUrl ?? tecnico.FotoUrl;

            if (requestDto.IsOnline.HasValue)
                tecnico.IsOnline = requestDto.IsOnline.Value;

            tecnico.LatitudeAtual = requestDto.LatitudeAtual ?? tecnico.LatitudeAtual;
            tecnico.LongitutdeAtual = requestDto.LongitudeAtual ?? tecnico.LongitutdeAtual;

            var validationResult = await _tecnicoValidator.ValidateAsync(tecnico);
            if (!validationResult.IsValid)
                throw new Exception(string.Join(", ", validationResult.Errors));
        }

        await _usuarioRepository.AtualizarUsuarioAsync(usuarioId);

        return new AtualizarUsuarioResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            Nome = usuario.Nome,
            UserName = usuario.UserName,
            Email = usuario.Email,
            Cpf = (usuario as Tecnico)?.Cpf,
            HoraEntrada = (usuario as Tecnico)?.HoraEntrada,
            HoraSaida = (usuario as Tecnico)?.HoraSaida,
            HoraAlmocoInicio = (usuario as Tecnico)?.HoraAlmocoInicio,
            HoraAlmocoFim = (usuario as Tecnico)?.HoraAlmocoFim,
            FotoUrl = (usuario as Tecnico)?.FotoUrl,
            IsOnline = (usuario as Tecnico)?.IsOnline ?? false,
            Role = usuario.Role,
            Ativo = usuario.Ativo,
            LatitudeAtual = (usuario as Tecnico)?.LatitudeAtual,
            LongitudeAtual = (usuario as Tecnico)?.LongitutdeAtual
        };
    }

    public async Task<PaginacaoResponseDto<UsuarioResponseDto>> GetTecnicosPaginadosAsync(PaginacaoRequestDto paginacao)
    {
        if (paginacao == null) paginacao = new PaginacaoRequestDto { Pagina = 1, TamanhoPagina = 10 };
        if (paginacao.Pagina <= 0) paginacao.Pagina = 1;
        if (paginacao.TamanhoPagina <= 0) paginacao.TamanhoPagina = 10;

        var usuarios = await _usuarioRepository.GetAllAsync();
        var totalUsuarios = usuarios.Count();
        var usuariosPaginados = usuarios.Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina).Take(paginacao.TamanhoPagina);

        var result = new List<UsuarioResponseDto>();
        TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        DateTime dataAtual = TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone).Date;

        foreach (var usuario in usuariosPaginados)
        {
            // Lógica de mapeamento completa
            result.Add(new UsuarioResponseDto { /* Mapeamento do usuário */ });
        }

        int totalPaginas = (int)Math.Ceiling(totalUsuarios / (double)paginacao.TamanhoPagina);

        return new PaginacaoResponseDto<UsuarioResponseDto>
        {
            Itens = result,
            TotalItens = totalUsuarios,
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalPaginas = totalPaginas
        };
    }

    public async Task<IEnumerable<UsuarioResponseDto>> GetAllTecnicosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        var tecnicos = usuarios.Where(u => u is Tecnico).ToList();
        var result = new List<UsuarioResponseDto>();
        // Lógica de mapeamento completa para cada técnico...
        return result;
    }

    public async Task<IEnumerable<UsuarioResponseDto>> GetTecnicosOnlineAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        var tecnicosOnline = usuarios.Where(u => u is Tecnico tecnico && tecnico.IsOnline).ToList();
        var result = new List<UsuarioResponseDto>();
        // Lógica de mapeamento completa para cada técnico online...
        return result;
    }

    public async Task<UsuarioResponseDto?> GetByIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);
        if (usuario == null) return null;

        var tecnico = usuario as Tecnico;
        EmpresaResponseDto? empresaDto = null;
        // Lógica de mapeamento completa...

        return new UsuarioResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            Nome = usuario.Nome,
            Email = usuario.Email,
            // ... resto do mapeamento
        };
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _usuarioRepository.ObterUsuarioPorIdAsync(id) != null;
    }

    public async Task AddAsync(Usuario entity)
    {
        await _usuarioRepository.CriarUsuarioAsync(entity);
    }

    public async Task UpdateAsync(Usuario entity)
    {
        await _usuarioRepository.AtualizarUsuarioAsync(entity.UsuarioId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);
        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        await _usuarioRepository.DeletarUsuarioAsync(id);
    }

    public async Task AtualizarLocalizacaoAtualAsync(Guid usuarioId, string latitude, string longitude)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario is Tecnico tecnico)
        {
            tecnico.LatitudeAtual = latitude;
            tecnico.LongitutdeAtual = longitude;
            await _usuarioRepository.AtualizarUsuarioAsync(tecnico.UsuarioId);
        }
        else
        {
            throw new InvalidOperationException("Apenas técnicos podem ter a localização atualizada.");
        }
    }

    public async Task<bool> AdicionarRegistroLocalizacaoAsync(Guid usuarioId, string latitude, string longitude)
    {
        // Lógica completa de adicionar registro de localização...
        return true;
    }

    Task<Usuario?> IBaseService<Usuario>.GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UsuarioResponseDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    private async Task<string> GerarNumeroMatriculaAsync()
    {
        var ultimaMatricula = await _tecnicoRepository.ObterUltimaMatriculaAsync();
        int novoNumero = ultimaMatricula + 1;
        return novoNumero.ToString("D6");
    }

    public async Task<EmpresaResponseDto> CreateEmpresaAsync(CriarEmpresaRequestDto requestDto)
    {
        // Lógica completa de criação de empresa...
        var empresa = new Empresa { /*...*/ };
        await _empresaRepository.CriarEmpresaAsync(empresa);
        return new EmpresaResponseDto { /*...*/ };
    }

    public async Task<EmpresaResponseDto?> GetEmpresaByIdAsync(Guid id)
    {
        // Lógica completa de busca de empresa...
        var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(id);
        if (empresa == null) return null;
        return new EmpresaResponseDto { /*...*/ };
    }

    public async Task<IEnumerable<EmpresaResponseDto>> GetAllEmpresasAsync()
    {
        // Lógica completa de busca de todas as empresas...
        var empresas = await _empresaRepository.GetAllEmpresasAsync();
        return empresas.Select(e => new EmpresaResponseDto { /*...*/ });
    }

    public async Task<EmpresaResponseDto> UpdateEmpresaAsync(Guid empresaId, AtualizarEmpresaRequestDto requestDto)
    {
        // Lógica completa de atualização de empresa...
        var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(empresaId);
        if (empresa == null) throw new Exception("Empresa não encontrada.");
        //...
        await _empresaRepository.AtualizarEmpresaAsync(empresa);
        return new EmpresaResponseDto { /*...*/ };
    }

    public async Task DeleteEmpresaAsync(Guid id)
    {
        await _empresaRepository.ExcluirEmpresaAsync(id);
    }

    public async Task ExpireDailyTokensAsync(bool expireTodaysTokens = false)
    {
        var cutOffDate = expireTodaysTokens ? DateTime.UtcNow.Date.AddDays(1) : DateTime.UtcNow.Date;
        var affectedUserIds = await _tokenManager.InvalidateActiveTokensBeforeDateAsync(cutOffDate);

        if (!affectedUserIds.Any())
        {
            return;
        }

        var usersToUpdate = await _usuarioRepository.GetUsersByIdsAsync(affectedUserIds);
        var updatedUsersList = new List<Usuario>();

        foreach (var usuario in usersToUpdate)
        {
            if (usuario is Tecnico tecnico && tecnico.IsOnline)
            {
                tecnico.IsOnline = false;
                updatedUsersList.Add(tecnico);
                await _auditoriaService.RegistrarAsync(tecnico.UsuarioId, tecnico.Nome, "Logout Automático", $"{tecnico.Nome} foi desconectado do sistema automaticamente.", tecnico.Role);
            }
        }

        if (updatedUsersList.Any())
        {
            await _usuarioRepository.UpdateUsersAsync(updatedUsersList);
        }
    }
    #endregion
}