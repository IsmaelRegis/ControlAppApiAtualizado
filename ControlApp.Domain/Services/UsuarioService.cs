using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Domain.Interfaces.Services;
using ControlApp.Domain.Validations;
using Microsoft.EntityFrameworkCore;

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

    #region Métodos de Autenticação e Gestão de Usuários
    public async Task<AutenticarUsuarioResponseDto> AuthenticateUsuarioAsync(AutenticarUsuarioRequestDto requestDto, string deviceInfo = null, string audience = "VibeService")
    {
        // Verifica se o UserName foi fornecido
        if (string.IsNullOrWhiteSpace(requestDto.UserName))
        {
            throw new UnauthorizedAccessException("Username deve ser fornecido");
        }
        // Autenticar por UserName
        var usuario = await _usuarioRepository.ObterUsuarioPorUserNameAsync(requestDto.UserName);
        if (usuario == null || !_cryptoSHA256.VerifyPassword(requestDto.Senha, usuario.Senha))
        {
            throw new UnauthorizedAccessException("Username ou senha inválidos");
        }
        // Marca técnico como online e atualiza a última autenticação
        bool isOnline = usuario is Tecnico tecnico ? (tecnico.IsOnline = true) : false;
        // Obtém o horário atual do Brasil (Brasília - GMT-3)
        TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        DateTime horarioBrasilia = TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone);
        usuario.DataHoraUltimaAutenticacao = horarioBrasilia;
        await _usuarioRepository.AtualizarUsuarioAsync(usuario.UsuarioId);

        var token = await _tokenManager.GenerateTokenAsync(
            usuario.UsuarioId,
            usuario.Role.ToString(),
            deviceInfo,
            audience);

        await _auditoriaService.RegistrarAsync(
     usuario.UsuarioId,
     usuario.Nome,
     "Entrou no sistema",
     $"{usuario.Nome} logou como {usuario.Role} às {usuario.DataHoraUltimaAutenticacao:dd/MM/yyyy HH:mm:ss}",
     usuario.Role

);

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

        // Se for técnico, marcar como offline
        if (usuario is Tecnico tecnico)
        {
            tecnico.IsOnline = false;
            await _usuarioRepository.AtualizarUsuarioAsync(tecnico.UsuarioId);
        }

        // Invalida os tokens ativos desse usuário (e opcionalmente o atual)
        await _tokenManager.InvalidateTokensForUserAsync(usuarioId, token);

        await _auditoriaService.RegistrarAsync(
     usuario.UsuarioId,
     usuario.Nome,
     "Saiu do sistema",
     $"{usuario.Nome} fez logout às {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
     usuario.Role
 );


    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _usuarioRepository.ObterUsuarioPorEmailAsync(email); // Busca usuário por email
    }

    public async Task ChangePasswordAsync(Guid usuarioId, string novaSenha)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        if (usuario is Administrador)
            throw new Exception("O Administrador não pode alterar a senha diretamente.");

        usuario.Senha = _cryptoSHA256.HashPassword(novaSenha); // Criptografa a nova senha
        await _usuarioRepository.AtualizarUsuarioAsync(usuarioId);
    }

    public async Task<CriarUsuarioResponseDto> CreateUsuarioAsync(CriarUsuarioRequestDto requestDto)
    {
        // Só permite criar técnicos
        if (requestDto.Role != UserRole.Colaborador)
            throw new Exception("Apenas técnicos podem ser criados por este serviço.");

        var usuarioExistente = await _usuarioRepository.ObterUsuarioPorCpfAsync(requestDto.Cpf);
        if (usuarioExistente != null)
            throw new Exception("CPF já cadastrado.");

        if (!string.IsNullOrWhiteSpace(requestDto.UserName))
        {
            var usuarioExistentePorUserName = await _usuarioRepository.ObterUsuarioPorUserNameAsync(requestDto.UserName);
            if (usuarioExistentePorUserName != null)
                throw new Exception("UserName já cadastrado. Escolha outro UserName.");
        }

        // Salva a foto, se tiver
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
            Role = requestDto.Role,
            Ativo = true,
            NumeroMatricula = await GerarNumeroMatriculaAsync(),
            EmpresaId = requestDto.EmpresaId
        };

        var validationResult = await _tecnicoValidator.ValidateAsync(tecnico);
        if (!validationResult.IsValid)
            throw new Exception(string.Join(", ", validationResult.Errors));

        tecnico.Senha = _cryptoSHA256.HashPassword(tecnico.Senha); // Criptografa a senha antes de salvar
        await _usuarioRepository.CriarUsuarioAsync(tecnico);

        // Busca dados da empresa, se tiver
        EmpresaResponseDto? empresaDto = null;
        if (requestDto.EmpresaId.HasValue)
        {
            var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(requestDto.EmpresaId.Value);
            if (empresa != null)
            {
                empresaDto = new EmpresaResponseDto
                {
                    EmpresaId = empresa.EmpresaId,
                    Ativo = empresa.Ativo,
                    NomeDaEmpresa = empresa.NomeDaEmpresa,
                    Endereco = empresa.Endereco == null ? null : new EnderecoDto
                    {
                        Cep = empresa.Endereco.Cep,
                        Logradouro = empresa.Endereco.Logradouro,
                        Complemento = empresa.Endereco.Complemento,
                        Bairro = empresa.Endereco.Bairro,
                        Localidade = empresa.Endereco.Cidade,
                        Uf = empresa.Endereco.Estado,
                        Numero = empresa.Endereco.Numero
                    },
                    Tecnicos = empresa.Tecnicos?.Select(t => new TecnicoResponseDto
                    {
                        UsuarioId = t.UsuarioId,
                        Nome = t.Nome,
                        Cpf = t.Cpf
                    }).ToList()
                };
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

    public async Task<AtualizarUsuarioResponseDto> UpdateUsuarioAsync(Guid usuarioId, AtualizarUsuarioRequestDto requestDto)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        if (usuario is Administrador)
            throw new Exception("O Administrador não pode ser atualizado diretamente.");

        // Atualiza só os campos que vieram no request
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
        #region Normalização da Paginação
        // 1) normaliza paginação
        if (paginacao == null) paginacao = new PaginacaoRequestDto { Pagina = 1, TamanhoPagina = 10 };
        if (paginacao.Pagina <= 0) paginacao.Pagina = 1;
        if (paginacao.TamanhoPagina <= 0) paginacao.TamanhoPagina = 10;
        #endregion

        #region Busca de Técnicos
        var usuarios = await _usuarioRepository.GetAllAsync(); // Pega todos os usuários

        // Calculando o total de usuários antes da paginação
        var totalUsuarios = usuarios.Count();

        // Aplicando a paginação na coleção de usuários
        var usuariosPaginados = usuarios
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina);
        #endregion

        #region Mapeamento
        var result = new List<UsuarioResponseDto>();

        // Obtenha a data atual no horário de Brasília
        TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        DateTime dataAtual = TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone).Date;

        foreach (var usuario in usuariosPaginados)
        {
            var tecnico = usuario as Tecnico;

            // Busca a empresa associada, se houver EmpresaId
            EmpresaResponseDto? empresaDto = null;
            if (tecnico?.EmpresaId.HasValue == true)
            {
                var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(tecnico.EmpresaId.Value);
                if (empresa != null)
                {
                    empresaDto = new EmpresaResponseDto
                    {
                        EmpresaId = empresa.EmpresaId,
                        NomeDaEmpresa = empresa.NomeDaEmpresa,
                        Ativo = empresa.Ativo
                    };
                }
            }

            // Buscar o trajeto do dia atual do técnico
            List<LocalizacaoResponseDto>? localizacoes = null;
            if (tecnico != null)
            {
                var trajetos = await _trajetoRepository.ObterTrajetosPorUsuarioAsync(tecnico.UsuarioId);
                var trajetosDoDia = trajetos?.Where(t => t.Data.Date == dataAtual).ToList();

                if (trajetosDoDia != null && trajetosDoDia.Any())
                {
                    var trajetoAtual = trajetosDoDia.OrderByDescending(t => t.Data).FirstOrDefault();
                    if (trajetoAtual != null)
                    {
                        var localizacoesTrajeto = await _localizacaoRepository.ObterLocalizacoesPorTrajetoIdAsync(trajetoAtual.Id);
                        localizacoes = localizacoesTrajeto.Select(l => new LocalizacaoResponseDto
                        {
                            LocalizacaoId = l.LocalizacaoId,
                            Latitude = l.Latitude,
                            Longitude = l.Longitude,
                            DataHora = l.DataHora,
                            Precisao = l.Precisao
                        }).ToList();
                    }
                }
            }

            result.Add(new UsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                Nome = usuario.Nome,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = usuario.Role,
                Ativo = usuario.Ativo,
                FotoUrl = usuario.FotoUrl ?? "URL_PADRAO_SEM_IMAGEM",
                TipoUsuario = usuario.TipoUsuario,
                Cpf = tecnico?.Cpf ?? "N/A",
                HoraEntrada = tecnico?.HoraEntrada ?? TimeSpan.Zero,
                HoraSaida = tecnico?.HoraSaida ?? TimeSpan.Zero,
                HoraAlmocoInicio = tecnico?.HoraAlmocoInicio ?? TimeSpan.Zero,
                HoraAlmocoFim = tecnico?.HoraAlmocoFim ?? TimeSpan.Zero,
                IsOnline = tecnico?.IsOnline ?? false,
                LatitudeAtual = tecnico?.LatitudeAtual,
                LongitudeAtual = tecnico?.LongitutdeAtual,
                DataEHoraLocalizacao = tecnico?.DataEHoraLocalizacao ?? DateTime.MinValue,
                DataHoraUltimaAutenticacao = usuario.DataHoraUltimaAutenticacao,
                NumeroMatricula = tecnico?.NumeroMatricula,
                EmpresaId = tecnico?.EmpresaId,
                NomeDaEmpresa = empresaDto?.NomeDaEmpresa,
                Empresa = empresaDto,
                Localizacoes = localizacoes
            });
        }
        #endregion

        #region Montagem da Resposta Paginada
        // 5) monta resposta paginada
        int totalPaginas = (int)Math.Ceiling(totalUsuarios / (double)paginacao.TamanhoPagina);

        return new PaginacaoResponseDto<UsuarioResponseDto>
        {
            Itens = result,
            TotalItens = totalUsuarios,
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalPaginas = totalPaginas
        };
        #endregion


    }

    public async Task<IEnumerable<UsuarioResponseDto>> GetAllTecnicosAsync()
    {
        #region Busca de Técnicos
        var usuarios = await _usuarioRepository.GetAllAsync(); // Pega todos os usuários
        var tecnicos = usuarios.Where(u => u is Tecnico).ToList();
        #endregion

        #region Mapeamento
        var result = new List<UsuarioResponseDto>();

        // Obtenha a data atual no horário de Brasília
        TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        DateTime dataAtual = TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone).Date;

        foreach (var usuario in tecnicos)
        {
            var tecnico = usuario as Tecnico;

            // Busca a empresa associada, se houver EmpresaId
            EmpresaResponseDto? empresaDto = null;
            if (tecnico?.EmpresaId.HasValue == true)
            {
                var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(tecnico.EmpresaId.Value);
                if (empresa != null)
                {
                    empresaDto = new EmpresaResponseDto
                    {
                        EmpresaId = empresa.EmpresaId,
                        NomeDaEmpresa = empresa.NomeDaEmpresa,
                        Ativo = empresa.Ativo
                    };
                }
            }

            // Buscar o trajeto do dia atual do técnico
            List<LocalizacaoResponseDto>? localizacoes = null;
            if (tecnico != null)
            {
                var trajetos = await _trajetoRepository.ObterTrajetosPorUsuarioAsync(tecnico.UsuarioId);
                var trajetosDoDia = trajetos?.Where(t => t.Data.Date == dataAtual).ToList();

                if (trajetosDoDia != null && trajetosDoDia.Any())
                {
                    var trajetoAtual = trajetosDoDia.OrderByDescending(t => t.Data).FirstOrDefault();
                    if (trajetoAtual != null)
                    {
                        var localizacoesTrajeto = await _localizacaoRepository.ObterLocalizacoesPorTrajetoIdAsync(trajetoAtual.Id);
                        localizacoes = localizacoesTrajeto.Select(l => new LocalizacaoResponseDto
                        {
                            LocalizacaoId = l.LocalizacaoId,
                            Latitude = l.Latitude,
                            Longitude = l.Longitude,
                            DataHora = l.DataHora,
                            Precisao = l.Precisao
                        }).ToList();
                    }
                }
            }

            result.Add(new UsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                Nome = usuario.Nome,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = usuario.Role,
                Ativo = usuario.Ativo,
                FotoUrl = usuario.FotoUrl ?? "URL_PADRAO_SEM_IMAGEM",
                TipoUsuario = usuario.TipoUsuario,
                Cpf = tecnico?.Cpf ?? "N/A",
                HoraEntrada = tecnico?.HoraEntrada ?? TimeSpan.Zero,
                HoraSaida = tecnico?.HoraSaida ?? TimeSpan.Zero,
                HoraAlmocoInicio = tecnico?.HoraAlmocoInicio ?? TimeSpan.Zero,
                HoraAlmocoFim = tecnico?.HoraAlmocoFim ?? TimeSpan.Zero,
                IsOnline = tecnico?.IsOnline ?? false,
                LatitudeAtual = tecnico?.LatitudeAtual,
                LongitudeAtual = tecnico?.LongitutdeAtual,
                DataEHoraLocalizacao = tecnico?.DataEHoraLocalizacao ?? DateTime.MinValue,
                DataHoraUltimaAutenticacao = usuario.DataHoraUltimaAutenticacao,
                NumeroMatricula = tecnico?.NumeroMatricula,
                EmpresaId = tecnico?.EmpresaId,
                NomeDaEmpresa = empresaDto?.NomeDaEmpresa,
                Empresa = empresaDto,
                Localizacoes = localizacoes
            });
        }
        #endregion

        return result;
    }

    public async Task<IEnumerable<UsuarioResponseDto>> GetTecnicosOnlineAsync()
    {
        #region Busca de Técnicos
        var usuarios = await _usuarioRepository.GetAllAsync(); // Pega todos os usuários

        // Filtra apenas os técnicos online
        var tecnicosOnline = usuarios
            .Where(u => u is Tecnico && ((Tecnico)u).IsOnline)
            .ToList();
        #endregion

        #region Mapeamento
        var result = new List<UsuarioResponseDto>();

        // Obtenha a data atual no horário de Brasília
        TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        DateTime dataAtual = TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone).Date;

        foreach (var usuario in tecnicosOnline)
        {
            var tecnico = usuario as Tecnico;

            // Busca a empresa associada, se houver EmpresaId
            EmpresaResponseDto? empresaDto = null;
            if (tecnico?.EmpresaId.HasValue == true)
            {
                var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(tecnico.EmpresaId.Value);
                if (empresa != null)
                {
                    empresaDto = new EmpresaResponseDto
                    {
                        EmpresaId = empresa.EmpresaId,
                        NomeDaEmpresa = empresa.NomeDaEmpresa,
                        Ativo = empresa.Ativo
                    };
                }
            }

            // Buscar o trajeto do dia atual do técnico
            List<LocalizacaoResponseDto>? localizacoes = null;
            if (tecnico != null)
            {
                var trajetos = await _trajetoRepository.ObterTrajetosPorUsuarioAsync(tecnico.UsuarioId);
                var trajetosDoDia = trajetos?.Where(t => t.Data.Date == dataAtual).ToList();

                if (trajetosDoDia != null && trajetosDoDia.Any())
                {
                    var trajetoAtual = trajetosDoDia.OrderByDescending(t => t.Data).FirstOrDefault();
                    if (trajetoAtual != null)
                    {
                        var localizacoesTrajeto = await _localizacaoRepository.ObterLocalizacoesPorTrajetoIdAsync(trajetoAtual.Id);
                        localizacoes = localizacoesTrajeto.Select(l => new LocalizacaoResponseDto
                        {
                            LocalizacaoId = l.LocalizacaoId,
                            Latitude = l.Latitude,
                            Longitude = l.Longitude,
                            DataHora = l.DataHora,
                            Precisao = l.Precisao
                        }).ToList();
                    }
                }
            }

            result.Add(new UsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                Nome = usuario.Nome,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = usuario.Role,
                Ativo = usuario.Ativo,
                FotoUrl = usuario.FotoUrl ?? "URL_PADRAO_SEM_IMAGEM",
                TipoUsuario = usuario.TipoUsuario,
                Cpf = tecnico?.Cpf ?? "N/A",
                HoraEntrada = tecnico?.HoraEntrada ?? TimeSpan.Zero,
                HoraSaida = tecnico?.HoraSaida ?? TimeSpan.Zero,
                HoraAlmocoInicio = tecnico?.HoraAlmocoInicio ?? TimeSpan.Zero,
                HoraAlmocoFim = tecnico?.HoraAlmocoFim ?? TimeSpan.Zero,
                IsOnline = tecnico?.IsOnline ?? false,
                LatitudeAtual = tecnico?.LatitudeAtual,
                LongitudeAtual = tecnico?.LongitutdeAtual,
                DataEHoraLocalizacao = tecnico?.DataEHoraLocalizacao ?? DateTime.MinValue,
                DataHoraUltimaAutenticacao = usuario.DataHoraUltimaAutenticacao,
                NumeroMatricula = tecnico?.NumeroMatricula,
                EmpresaId = tecnico?.EmpresaId,
                NomeDaEmpresa = empresaDto?.NomeDaEmpresa,
                Empresa = empresaDto,
                Localizacoes = localizacoes
            });
        }
        #endregion

        return result;
    }
    public async Task<UsuarioResponseDto?> GetByIdAsync(Guid id)
    {
        #region Busca de Técnico
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);
        if (usuario == null)
            return null;
        #endregion

        #region Mapeamento
        var tecnico = usuario as Tecnico;

        // Obtenha a data atual no horário de Brasília
        TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        DateTime dataAtual = TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone).Date;

        // Busca a empresa associada, se houver EmpresaId
        EmpresaResponseDto? empresaDto = null;
        if (tecnico?.EmpresaId.HasValue == true)
        {
            var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(tecnico.EmpresaId.Value);
            if (empresa != null)
            {
                empresaDto = new EmpresaResponseDto
                {
                    EmpresaId = empresa.EmpresaId,
                    NomeDaEmpresa = empresa.NomeDaEmpresa,
                    Ativo = empresa.Ativo
                };
            }
        }

        // Buscar o trajeto do dia atual do técnico
        List<LocalizacaoResponseDto>? localizacoes = null;
        if (tecnico != null)
        {
            var trajetos = await _trajetoRepository.ObterTrajetosPorUsuarioAsync(tecnico.UsuarioId);
            var trajetosDoDia = trajetos?.Where(t => t.Data.Date == dataAtual).ToList();

            if (trajetosDoDia != null && trajetosDoDia.Any())
            {
                var trajetoAtual = trajetosDoDia.OrderByDescending(t => t.Data).FirstOrDefault();
                if (trajetoAtual != null)
                {
                    var localizacoesTrajeto = await _localizacaoRepository.ObterLocalizacoesPorTrajetoIdAsync(trajetoAtual.Id);
                    localizacoes = localizacoesTrajeto.Select(l => new LocalizacaoResponseDto
                    {
                        LocalizacaoId = l.LocalizacaoId,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        DataHora = l.DataHora,
                        Precisao = l.Precisao
                    }).ToList();
                }
            }
        }

        return new UsuarioResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            Nome = usuario.Nome,
            Email = usuario.Email,
            UserName = usuario.UserName,
            Role = usuario.Role,
            Ativo = usuario.Ativo,
            FotoUrl = usuario.FotoUrl ?? "URL_PADRAO_SEM_IMAGEM",
            TipoUsuario = usuario.TipoUsuario,
            Cpf = tecnico?.Cpf ?? "N/A",
            HoraEntrada = tecnico?.HoraEntrada ?? TimeSpan.Zero,
            HoraSaida = tecnico?.HoraSaida ?? TimeSpan.Zero,
            HoraAlmocoInicio = tecnico?.HoraAlmocoInicio ?? TimeSpan.Zero,
            HoraAlmocoFim = tecnico?.HoraAlmocoFim ?? TimeSpan.Zero,
            IsOnline = tecnico?.IsOnline ?? false,
            LatitudeAtual = tecnico?.LatitudeAtual,
            LongitudeAtual = tecnico?.LongitutdeAtual,
            DataEHoraLocalizacao = tecnico?.DataEHoraLocalizacao ?? DateTime.MinValue,
            DataHoraUltimaAutenticacao = usuario.DataHoraUltimaAutenticacao,
            NumeroMatricula = tecnico?.NumeroMatricula,
            EmpresaId = tecnico?.EmpresaId,
            Empresa = empresaDto, // Inclui os dados da empresa, com NomeDaEmpresa
            NomeDaEmpresa = empresaDto?.NomeDaEmpresa,
            Localizacoes = localizacoes
        };
        #endregion
    }
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _usuarioRepository.ObterUsuarioPorIdAsync(id) != null; // Verifica se o usuário existe
    }

    public async Task AddAsync(Usuario entity)
    {
        if (entity is Administrador)
            throw new Exception("O Administrador não pode ser criado pelo sistema.");

        await _usuarioRepository.CriarUsuarioAsync(entity);
    }

    public async Task UpdateAsync(Usuario entity)
    {
        if (entity is Administrador)
            throw new Exception("O Administrador não pode ser atualizado.");

        await _usuarioRepository.AtualizarUsuarioAsync(entity.UsuarioId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);
        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        if (usuario is Administrador)
            throw new Exception("O Administrador não pode ser excluído.");

        await _usuarioRepository.DeletarUsuarioAsync(id); // Deleta o usuário
    }

    public async Task AtualizarLocalizacaoAtualAsync(Guid usuarioId, string latitude, string longitude)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        if (usuario is Tecnico tecnico)
        {
            tecnico.LatitudeAtual = latitude;
            tecnico.LongitutdeAtual = longitude;
            await _usuarioRepository.AtualizarUsuarioAsync(tecnico.UsuarioId); // Atualiza a localização do técnico
        }
        else
            throw new InvalidOperationException("Apenas técnicos podem ter a localização atualizada.");
    }

    public async Task<bool> AdicionarRegistroLocalizacaoAsync(Guid usuarioId, string latitude, string longitude)
    {
        try
        {
            var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            if (usuario is Tecnico tecnico)
            {
                // Usando TimeZoneInfo para obter a data/hora atual no horário de Brasília
                TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime dataHoraLocal = TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone);

                // Verificar se há trajetos de dias anteriores e excluir suas localizações
                var trajetos = await _trajetoRepository.ObterTrajetosPorUsuarioAsync(tecnico.UsuarioId);

                // Limpar localizações e trajetos de dias anteriores
                if (trajetos != null && trajetos.Any())
                {
                    foreach (var trajeto in trajetos)
                    {
                        // Se o trajeto for de um dia anterior ao atual
                        if (trajeto.Data.Date < dataHoraLocal.Date)
                        {
                            // Excluir localizações deste trajeto
                            await _localizacaoRepository.ExcluirLocalizacoesPorTrajetoIdAsync(trajeto.Id);

                            // Opcional: Você pode decidir manter o trajeto, mas sem localizações
                            // Ou excluir o trajeto completo:
                            // await _trajetoRepository.DeleteAsync(trajeto.Id);
                        }
                    }
                }

                // Verifica se já existe um trajeto para hoje
                var trajetoHoje = trajetos?.FirstOrDefault(t => t.Data.Date == dataHoraLocal.Date);
                Guid trajetoId;

                // Se não existir trajeto para hoje, cria um novo
                if (trajetoHoje == null)
                {
                    var novoTrajeto = new Trajeto
                    {
                        Id = Guid.NewGuid(),
                        Data = dataHoraLocal,
                        UsuarioId = tecnico.UsuarioId,
                        Status = "Em andamento",
                        DistanciaTotalKm = 0,
                        DuracaoTotal = TimeSpan.Zero
                    };

                    await _trajetoRepository.AddAsync(novoTrajeto);
                    trajetoId = novoTrajeto.Id;
                }
                else
                {
                    // Usa o trajeto existente para hoje
                    trajetoId = trajetoHoje.Id;
                }

                // Cria uma nova localização
                var localizacao = new Localizacao
                {
                    LocalizacaoId = Guid.NewGuid(),
                    Latitude = latitude,
                    Longitude = longitude,
                    DataHora = dataHoraLocal,
                    Precisao = 0,
                    TrajetoId = trajetoId
                };

                // Adiciona a localização
                return await _localizacaoRepository.AdicionarLocalizacaoAsync(localizacao);
            }
            else
            {
                throw new InvalidOperationException("Apenas técnicos podem ter a localização registrada.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar localização: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Exceção interna: {ex.InnerException.Message}");

            return false;
        }
    }
    Task<Usuario?> IBaseService<Usuario>.GetByIdAsync(Guid id)
    {
        throw new NotImplementedException(); // Método não implementado
    }

    public Task<IEnumerable<UsuarioResponseDto>> GetAllAsync()
    {
        throw new NotImplementedException(); // Método não implementado
    }

    private async Task<string> GerarNumeroMatriculaAsync()
    {
        var ultimaMatricula = await _tecnicoRepository.ObterUltimaMatriculaAsync();
        int novoNumero = ultimaMatricula + 1;
        return novoNumero.ToString("D6"); // Gera número de matrícula com 6 dígitos (ex: "000001")
    }

    public async Task<EmpresaResponseDto> CreateEmpresaAsync(CriarEmpresaRequestDto requestDto)
    {
        var empresa = new Empresa
        {
            EmpresaId = Guid.NewGuid(),
            NomeDaEmpresa = requestDto.NomeDaEmpresa,
            DataCriacao = DateTime.Now,
            Ativo = true
        };

        // Adiciona endereço se tiver CEP
        if (requestDto.Endereco != null && !string.IsNullOrEmpty(requestDto.Endereco.Cep))
        {
            empresa.Endereco = new Endereco
            {
                EnderecoId = Guid.NewGuid(),
                Cep = requestDto.Endereco.Cep,
                Numero = requestDto.Endereco.Numero,
                Complemento = requestDto.Endereco.Complemento,
                Logradouro = requestDto.Endereco.Logradouro,
                Bairro = requestDto.Endereco.Bairro,
                Cidade = requestDto.Endereco.Cidade,
                Estado = requestDto.Endereco.Estado

            };
        }

        await _empresaRepository.CriarEmpresaAsync(empresa);

        return new EmpresaResponseDto
        {
            EmpresaId = empresa.EmpresaId,
            NomeDaEmpresa = empresa.NomeDaEmpresa,
            Ativo = empresa.Ativo,
            DataCriacao = empresa.DataCriacao,
            Endereco = empresa.Endereco == null ? null : new EnderecoDto
            {
                Cep = empresa.Endereco.Cep,
                Numero = empresa.Endereco.Numero,
                Complemento = empresa.Endereco.Complemento,
                Logradouro = empresa.Endereco.Logradouro,
                Bairro = empresa.Endereco.Bairro,
                Localidade = empresa.Endereco.Cidade,
                Uf = empresa.Endereco.Estado
            },
            Tecnicos = null
        };
    }

    public async Task<EmpresaResponseDto?> GetEmpresaByIdAsync(Guid id)
    {
        var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(id);
        if (empresa == null)
            return null;

        return new EmpresaResponseDto
        {
            EmpresaId = empresa.EmpresaId,
            NomeDaEmpresa = empresa.NomeDaEmpresa,
            Ativo = empresa.Ativo,
            DataCriacao = empresa.DataCriacao,
            Endereco = new EnderecoDto
            {
                Cep = empresa.Endereco?.Cep,
                Logradouro = empresa.Endereco?.Logradouro,
                Bairro = empresa.Endereco?.Bairro,
                Localidade = empresa.Endereco?.Cidade,
                Uf = empresa.Endereco?.Estado,
                Complemento = empresa.Endereco?.Complemento,
                Numero = empresa.Endereco?.Numero
            },
            Tecnicos = empresa.Tecnicos?.Select(t => new TecnicoResponseDto
            {
                UsuarioId = t.UsuarioId,
                Nome = t.Nome,
                Cpf = t.Cpf,
                Role = t.Role
            }).ToList()
        };
    }

    public async Task<IEnumerable<EmpresaResponseDto>> GetAllEmpresasAsync()
    {
        var empresas = await _empresaRepository.GetAllEmpresasAsync(); // Pega todas as empresas
        return empresas.Select(e => new EmpresaResponseDto
        {
            EmpresaId = e.EmpresaId,
            NomeDaEmpresa = e.NomeDaEmpresa,
            Ativo = e.Ativo,
            DataCriacao = e.DataCriacao,
            Endereco = new EnderecoDto
            {
                Cep = e.Endereco?.Cep,
                Logradouro = e.Endereco?.Logradouro,
                Bairro = e.Endereco?.Bairro,
                Localidade = e.Endereco?.Cidade,
                Uf = e.Endereco?.Estado,
                Complemento = e.Endereco?.Complemento,
                Numero = e.Endereco?.Numero
            },
            Tecnicos = e.Tecnicos?.Select(t => new TecnicoResponseDto
            {
                UsuarioId = t.UsuarioId,
                Nome = t.Nome,
                Cpf = t.Cpf,
                Role = t.Role
            }).ToList()
        });
    }

    public async Task<EmpresaResponseDto> UpdateEmpresaAsync(Guid empresaId, AtualizarEmpresaRequestDto requestDto)
    {
        var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(empresaId);
        if (empresa == null)
            throw new Exception("Empresa não encontrada.");

        empresa.NomeDaEmpresa = requestDto.NomeDaEmpresa ?? empresa.NomeDaEmpresa;

        // Atualiza o endereço se vier no request
        if (requestDto.Endereco != null)
        {
            if (empresa.Endereco == null)
                empresa.Endereco = new Endereco { EnderecoId = Guid.NewGuid() };
            empresa.Endereco.Cep = requestDto.Endereco.Cep ?? empresa.Endereco.Cep;
            empresa.Endereco.Numero = requestDto.Endereco.Numero ?? empresa.Endereco.Numero;
            empresa.Endereco.Complemento = requestDto.Endereco.Complemento ?? empresa.Endereco.Complemento;
            empresa.Endereco.Logradouro = requestDto.Endereco.Logradouro ?? empresa.Endereco.Logradouro;
            empresa.Endereco.Bairro = requestDto.Endereco.Bairro ?? empresa.Endereco.Bairro;
            empresa.Endereco.Cidade = requestDto.Endereco.Cidade ?? empresa.Endereco.Cidade;
            empresa.Endereco.Estado = requestDto.Endereco.Estado ?? empresa.Endereco.Estado;
        }

        await _empresaRepository.AtualizarEmpresaAsync(empresa);

        return new EmpresaResponseDto
        {
            EmpresaId = empresa.EmpresaId,
            NomeDaEmpresa = empresa.NomeDaEmpresa,
            Ativo = empresa.Ativo,
            DataCriacao = empresa.DataCriacao,
            Endereco = empresa.Endereco == null ? null : new EnderecoDto
            {
                Cep = empresa.Endereco.Cep,
                Numero = empresa.Endereco.Numero,
                Complemento = empresa.Endereco.Complemento,
                Logradouro = empresa.Endereco.Logradouro,
                Bairro = empresa.Endereco.Bairro,
                Localidade = empresa.Endereco.Cidade,
                Uf = empresa.Endereco.Estado
            },
            Tecnicos = empresa.Tecnicos?.Select(t => new TecnicoResponseDto
            {
                UsuarioId = t.UsuarioId,
                Nome = t.Nome,
                Cpf = t.Cpf,
                Role = t.Role
            }).ToList()
        };
    }

    public async Task DeleteEmpresaAsync(Guid id)
    {
        await _empresaRepository.ExcluirEmpresaAsync(id);
    }

    #endregion

    // Dentro da classe UsuarioService
    // Dentro da classe UsuarioService

    public async Task ExpireDailyTokensAsync(bool expireTodaysTokens = false)
    {
        // Define a data de corte usando UTC.
        // Esta é a forma mais segura e recomendada.
        var cutOffDate = expireTodaysTokens
            ? DateTime.UtcNow.Date.AddDays(1) // Para o teste, inclui todos os tokens criados hoje.
            : DateTime.UtcNow.Date;          // Para a rotina normal, o corte é à meia-noite UTC de hoje.

        // 1. Invalida os tokens criados antes da data de corte e obtém os IDs dos usuários.
        var affectedUserIds = await _tokenManager.InvalidateActiveTokensBeforeDateAsync(cutOffDate);

        if (!affectedUserIds.Any())
        {
            return; // Nenhum usuário para processar.
        }

        // 2. Busca todos os usuários afetados de uma só vez.
        var usersToUpdate = await _usuarioRepository.GetUsersByIdsAsync(affectedUserIds);

        var updatedUsersList = new List<Usuario>();

        // 3. Altera o status 'IsOnline' para os técnicos e registra a auditoria.
        foreach (var usuario in usersToUpdate)
        {
            if (usuario is Tecnico tecnico && tecnico.IsOnline)
            {
                tecnico.IsOnline = false;
                updatedUsersList.Add(tecnico);

                await _auditoriaService.RegistrarAsync(
                    tecnico.UsuarioId,
                    tecnico.Nome,
                    "Logout Automático",
                    $"{tecnico.Nome} foi desconectado do sistema automaticamente.",
                    tecnico.Role
                );
            }
        }

        // 4. Salva todas as atualizações de usuários no banco de uma vez.
        if (updatedUsersList.Any())
        {
            await _usuarioRepository.UpdateUsersAsync(updatedUsersList);
        }
    }

}