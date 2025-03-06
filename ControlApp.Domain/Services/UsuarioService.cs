using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Domain.Interfaces.Services;
using ControlApp.Domain.Validations;
using System;
using System.Collections.Generic;
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

    public UsuarioService(IUsuarioRepository usuarioRepository, ITokenSecurity tokenSecurity, IImageService imageService, ITecnicoRepository tecnicoRepository, IEmpresaRepository empresaRepository)
    {
        _usuarioRepository = usuarioRepository;
        _tokenSecurity = tokenSecurity;
        _imageService = imageService;
        _usuarioValidator = new UsuarioValidator();
        _tecnicoValidator = new TecnicoValidator();
        _cryptoSHA256 = new CryptoSHA256();
        _tecnicoRepository = tecnicoRepository;
        _empresaRepository = empresaRepository;
    }
    public async Task<AutenticarUsuarioResponseDto> AuthenticateUsuarioAsync(AutenticarUsuarioRequestDto requestDto)
    {

        if (!string.IsNullOrWhiteSpace(requestDto.UserName))
        {
            var usuario = await _usuarioRepository.ObterUsuarioPorUserNameAsync(requestDto.UserName);

            if (usuario == null || !_cryptoSHA256.VerifyPassword(requestDto.Senha, usuario.Senha))
            {
                throw new UnauthorizedAccessException("UserName ou senha inválidos.");
            }

            bool isOnline = false;
            if (usuario is Tecnico tecnico)
            {
                tecnico.IsOnline = true;
                isOnline = true;
            }

            usuario.DataHoraUltimaAutenticacao = DateTime.Now;

            await _usuarioRepository.AtualizarUsuarioAsync(usuario.UsuarioId);

            var token = _tokenSecurity.CreateToken(usuario.UsuarioId, usuario.Role.ToString());

            return new AutenticarUsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Role = usuario.Role.ToString(),
                Cpf = (usuario as Tecnico)?.Cpf,
                Token = token,
                FotoUrl = usuario.FotoUrl,
                IsOnline = isOnline,
                DataHoraAutenticacao = usuario.DataHoraUltimaAutenticacao ?? DateTime.MinValue

            };
        }

        else if (!string.IsNullOrWhiteSpace(requestDto.Cpf))
        {
            var usuario = await _usuarioRepository.ObterUsuarioPorCpfAsync(requestDto.Cpf);

            if (usuario == null || !_cryptoSHA256.VerifyPassword(requestDto.Senha, usuario.Senha))
            {
                throw new UnauthorizedAccessException("CPF ou senha inválidos.");
            }

            bool isOnline = false;
            if (usuario is Tecnico tecnico)
            {
                tecnico.IsOnline = true;
                isOnline = true;
            }

            usuario.DataHoraUltimaAutenticacao = DateTime.Now;

            await _usuarioRepository.AtualizarUsuarioAsync(usuario.UsuarioId);

            var token = _tokenSecurity.CreateToken(usuario.UsuarioId, usuario.Role.ToString());

            return new AutenticarUsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Role = usuario.Role.ToString(),
                Cpf = (usuario as Tecnico)?.Cpf,
                Token = token,
                FotoUrl = usuario.FotoUrl,
                IsOnline = isOnline,
                DataHoraAutenticacao = usuario.DataHoraUltimaAutenticacao ?? DateTime.MinValue

            };
        }
        else
        {
            throw new UnauthorizedAccessException("CPF ou UserName deve ser fornecido.");
        }
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _usuarioRepository.ObterUsuarioPorEmailAsync(email);
    }

    public async Task ChangePasswordAsync(Guid usuarioId, string novaSenha)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new Exception("Usuário não encontrado.");
        }

        if (usuario is Administrador)
        {
            throw new Exception("O Administrador não pode alterar a senha diretamente.");
        }

        usuario.Senha = _cryptoSHA256.HashPassword(novaSenha);
        await _usuarioRepository.AtualizarUsuarioAsync(usuarioId);
    }

    public async Task<CriarUsuarioResponseDto> CreateUsuarioAsync(CriarUsuarioRequestDto requestDto)
    {
        if (requestDto.Role != UserRole.Colaborador)
            throw new Exception("Apenas técnicos podem ser criados por este serviço.");

        var usuarioExistente = await _usuarioRepository.ObterUsuarioPorCpfAsync(requestDto.Cpf);
        if (usuarioExistente != null)
            throw new Exception("CPF já cadastrado.");

        string? fotoUrl = null;
        if (requestDto.FotoFile != null)
            fotoUrl = await _imageService.UploadImageAsync(requestDto.FotoFile);

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

        tecnico.Senha = _cryptoSHA256.HashPassword(tecnico.Senha);
        await _usuarioRepository.CriarUsuarioAsync(tecnico);

        // Buscar dados da empresa usando o repositório
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
        {
            throw new Exception("Usuário não encontrado.");
        }

        if (usuario is Administrador)
        {
            throw new Exception("O Administrador não pode ser atualizado diretamente.");
        }

        usuario.Nome = requestDto.Nome ?? usuario.Nome;
        usuario.UserName = requestDto.UserName ?? usuario.UserName;
        usuario.Email = requestDto.Email ?? usuario.Email;

        if (!string.IsNullOrEmpty(requestDto.Senha))
        {
            usuario.Senha = _cryptoSHA256.HashPassword(requestDto.Senha);
        }

        if (usuario is Tecnico tecnico)
        {
            tecnico.HoraEntrada = requestDto.HoraEntrada ?? tecnico.HoraEntrada;
            tecnico.HoraSaida = requestDto.HoraSaida ?? tecnico.HoraSaida;
            tecnico.HoraAlmocoInicio = requestDto.HoraAlmocoInicio ?? tecnico.HoraAlmocoInicio;
            tecnico.HoraAlmocoFim = requestDto.HoraAlmocoFim ?? tecnico.HoraAlmocoFim;
            tecnico.Cpf = requestDto.Cpf ?? tecnico.Cpf;

            if (requestDto.FotoFile != null)
            {
                var fotoUrl = await _imageService.UploadImageAsync(requestDto.FotoFile);
                tecnico.FotoUrl = fotoUrl;
            }
            else
            {
                tecnico.FotoUrl = requestDto.FotoUrl ?? tecnico.FotoUrl;
            }

            if (requestDto.IsOnline.HasValue)
            {
                tecnico.IsOnline = requestDto.IsOnline.Value;
            }

            tecnico.LatitudeAtual = requestDto.LatitudeAtual ?? tecnico.LatitudeAtual;
            tecnico.LongitutdeAtual = requestDto.LongitudeAtual ?? tecnico.LongitutdeAtual;

            var validationResult = await _tecnicoValidator.ValidateAsync(tecnico);
            if (!validationResult.IsValid)
            {
                throw new Exception(string.Join(", ", validationResult.Errors));
            }
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

    public async Task<IEnumerable<UsuarioResponseDto>> GetAllTecnicosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();

        return usuarios.Select(usuario =>
        {
            var tecnico = usuario as Tecnico;

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
                DataHoraUltimaAutenticacao = usuario.DataHoraUltimaAutenticacao,
                            NumeroMatricula = tecnico?.NumeroMatricula,
            EmpresaId = tecnico?.EmpresaId
            };
        }).ToList();
    }
    public async Task<UsuarioResponseDto?> GetByIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);
        if (usuario == null)
            return null;

        var tecnico = usuario as Tecnico;

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
            DataHoraUltimaAutenticacao = usuario.DataHoraUltimaAutenticacao,
            NumeroMatricula = tecnico?.NumeroMatricula,
            EmpresaId = tecnico?.EmpresaId
           
        };
    }
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _usuarioRepository.ObterUsuarioPorIdAsync(id) != null;
    }

    public async Task AddAsync(Usuario entity)
    {
        if (entity is Administrador)
        {
            throw new Exception("O Administrador não pode ser criado pelo sistema.");
        }

        await _usuarioRepository.CriarUsuarioAsync(entity);
    }

    public async Task UpdateAsync(Usuario entity)
    {
        if (entity is Administrador)
        {
            throw new Exception("O Administrador não pode ser atualizado.");
        }

        await _usuarioRepository.AtualizarUsuarioAsync(entity.UsuarioId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);
        if (usuario == null)
        {
            throw new Exception("Usuário não encontrado.");
        }

        if (usuario is Administrador)
        {
            throw new Exception("O Administrador não pode ser excluído.");
        }
        await _usuarioRepository.DeletarUsuarioAsync(id);
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

            await _usuarioRepository.AtualizarUsuarioAsync(tecnico.UsuarioId);
        }
        else
        {
            throw new InvalidOperationException("Apenas técnicos podem ter a localização atualizada.");
        }
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
        return novoNumero.ToString("D6"); // Formata como "000001", "000002", etc.
    }

    public async Task<EmpresaResponseDto> CreateEmpresaAsync(CriarEmpresaRequestDto requestDto)
    {
        var empresa = new Empresa
        {
            EmpresaId = Guid.NewGuid(),
            NomeDaEmpresa = requestDto.NomeDaEmpresa,
            Ativo = true
        };

        if (requestDto.Endereco != null && !string.IsNullOrEmpty(requestDto.Endereco.Cep))
        {
            // Aqui você pode integrar com um serviço como ViaCep para buscar os dados do endereço pelo CEP
            // Por simplicidade, assumimos que apenas Cep, Numero e Complemento são fornecidos
            empresa.Endereco = new Endereco
            {
                EnderecoId = Guid.NewGuid(),
                Cep = requestDto.Endereco.Cep,
                Numero = requestDto.Endereco.Numero,
                Complemento = requestDto.Endereco.Complemento
                // Outros campos como Logradouro, Bairro, etc., podem ser preenchidos via integração externa se desejar
            };
        }

        await _empresaRepository.CriarEmpresaAsync(empresa);

        return new EmpresaResponseDto
        {
            EmpresaId = empresa.EmpresaId,
            NomeDaEmpresa = empresa.NomeDaEmpresa,
            Ativo = empresa.Ativo,
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
            Tecnicos = null // Não há técnicos associados inicialmente
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
                Cpf = t.Cpf
            }).ToList()
        };

    }
    public async Task<IEnumerable<EmpresaResponseDto>> GetAllEmpresasAsync()
    {
        var empresas = await _empresaRepository.GetAllEmpresasAsync();
        return empresas.Select(e => new EmpresaResponseDto
        {
            EmpresaId = e.EmpresaId,
            NomeDaEmpresa = e.NomeDaEmpresa,
            Ativo = e.Ativo,
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
                Cpf = t.Cpf
            }).ToList()
        });
    }
    public async Task<EmpresaResponseDto> UpdateEmpresaAsync(Guid empresaId, AtualizarEmpresaRequestDto requestDto)
    {
        var empresa = await _empresaRepository.ObterEmpresaPorIdAsync(empresaId);
        if (empresa == null)
            throw new Exception("Empresa não encontrada.");

        empresa.NomeDaEmpresa = requestDto.NomeDaEmpresa ?? empresa.NomeDaEmpresa;

        if (requestDto.Endereco != null)
        {
            if (empresa.Endereco == null)
                empresa.Endereco = new Endereco { EnderecoId = Guid.NewGuid() };

            empresa.Endereco.Cep = requestDto.Endereco.Cep ?? empresa.Endereco.Cep;
            empresa.Endereco.Numero = requestDto.Endereco.Numero ?? empresa.Endereco.Numero;
            empresa.Endereco.Complemento = requestDto.Endereco.Complemento ?? empresa.Endereco.Complemento;
            // Outros campos permanecem como estão, a menos que sejam atualizados via integração externa
        }

        await _empresaRepository.AtualizarEmpresaAsync(empresa);

        return new EmpresaResponseDto
        {
            EmpresaId = empresa.EmpresaId,
            NomeDaEmpresa = empresa.NomeDaEmpresa,
            Ativo = empresa.Ativo,
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
                Cpf = t.Cpf
            }).ToList()
        };
    }
    public async Task DeleteEmpresaAsync(Guid id)
    {
        await _empresaRepository.ExcluirEmpresaAsync(id);
    }

}