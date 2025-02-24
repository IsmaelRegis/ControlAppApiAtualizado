using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PontoService : IPontoService
{
    private readonly IPontoRepository _pontoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IImageService _imageService;

    public PontoService(
        IPontoRepository pontoRepository,
        IUsuarioRepository usuarioRepository,
        IImageService imageService)
    {
        _pontoRepository = pontoRepository;
        _usuarioRepository = usuarioRepository;
        _imageService = imageService;
    }

    public async Task<RegistrarInicioExpedienteResponseDto> RegisterInicioExpedienteAsync(Guid usuarioId, RegistrarInicioExpedienteRequestDto dto)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var pontosHoje = await _pontoRepository.ObterPontosPorUsuarioEPeriodoAsync(
            usuarioId,
            DateTime.Now.Date,
            DateTime.Now.Date.AddDays(1));

        bool jaTemExpedienteHoje = pontosHoje.Any(p => p.TipoPonto == TipoPonto.Expediente);
        if (jaTemExpedienteHoje)
            throw new InvalidOperationException("Já existe um ponto de expediente registrado para hoje para este usuário.");

        if (dto.FotoInicioExpedienteFile != null)
        {
            var fotoInicioUrl = await _imageService.UploadImageAsync(dto.FotoInicioExpedienteFile);
            dto.FotoInicioExpediente = fotoInicioUrl;
        }

        var novoPonto = new Ponto
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            TipoPonto = TipoPonto.Expediente,
            InicioExpediente = DateTime.Now,
            LatitudeInicioExpediente = string.IsNullOrWhiteSpace(dto.Latitude) ? "0" : dto.Latitude.Replace(",", "."),
            LongitudeInicioExpediente = string.IsNullOrWhiteSpace(dto.Longitude) ? "0" : dto.Longitude.Replace(",", "."),
            ObservacaoInicioExpediente = dto.Observacoes,
            FotoInicioExpediente = dto.FotoInicioExpediente,
            Ativo = true
        };

        await _pontoRepository.CriarPontoAsync(novoPonto);

        return new RegistrarInicioExpedienteResponseDto
        {
            PontoId = novoPonto.Id,
            Latitude = dto.Latitude,  
            Longitude = dto.Longitude, 
            FotoInicioExpediente = dto.FotoInicioExpediente,
            Observacoes = dto.Observacoes,
            InicioExpediente = DateTime.Now
        };
    }

    public async Task<RegistrarFimExpedienteResponseDto> RegisterFimExpedienteAsync(Guid usuarioId, Guid pontoId, RegistrarFimExpedienteRequestDto dto)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var ponto = await _pontoRepository.ObterPontoPorIdAsync(pontoId);
        if (ponto == null || ponto.TipoPonto != TipoPonto.Expediente)
            throw new InvalidOperationException("Ponto de expediente não encontrado.");

        if (ponto.InicioExpediente == null)
            throw new InvalidOperationException("Início de expediente não registrado.");

        if (dto.FotoFimExpedienteFile != null)
        {
            var fotoFimUrl = await _imageService.UploadImageAsync(dto.FotoFimExpedienteFile);
            dto.FotoFimExpediente = fotoFimUrl;
        }

        var fimReal = DateTime.Now;

        if (fimReal <= ponto.InicioExpediente)
            throw new InvalidOperationException("O horário de fim do expediente não pode ser anterior ou igual ao início.");

        var horasTrabalhadas = fimReal - ponto.InicioExpediente.Value;

        var tecnico = usuario as Tecnico;
        if (tecnico == null)
            throw new InvalidOperationException("Usuário não é um Técnico.");

        var jornadaDiaria = tecnico.HoraSaida - tecnico.HoraEntrada;
        var intervaloAlmoco = tecnico.HoraAlmocoFim - tecnico.HoraAlmocoInicio;

        var jornadaEfetiva = jornadaDiaria - intervaloAlmoco;
        var horasExtras = horasTrabalhadas > jornadaEfetiva
            ? horasTrabalhadas - jornadaEfetiva
            : TimeSpan.Zero;

        var horasDevidas = horasTrabalhadas < jornadaEfetiva
            ? jornadaEfetiva - horasTrabalhadas
            : TimeSpan.Zero;

        if (horasExtras.TotalHours > 24)
            horasExtras = TimeSpan.FromHours(24);
        if (horasDevidas.TotalHours > 24)
            horasDevidas = TimeSpan.FromHours(24);

        ponto.FimExpediente = fimReal;
        ponto.HorasTrabalhadas = horasTrabalhadas;
        ponto.HorasExtras = horasExtras;
        ponto.HorasDevidas = horasDevidas;
        ponto.LatitudeFimExpediente = string.IsNullOrWhiteSpace(dto.Latitude) ? "0" : dto.Latitude.Replace(",", ".");
        ponto.LongitudeFimExpediente = string.IsNullOrWhiteSpace(dto.Longitude) ? "0" : dto.Longitude.Replace(",", ".");
        ponto.FotoFimExpediente = dto.FotoFimExpediente;
        ponto.ObservacaoFimExpediente = dto.Observacoes;
        ponto.Ativo = true;

        await _pontoRepository.AtualizarPontoAsync(ponto.Id, ponto);

        return new RegistrarFimExpedienteResponseDto
        {
            HorasTrabalhadas = horasTrabalhadas,
            HorasExtras = horasExtras,
            HorasDevidas = horasDevidas,
            DistanciaTotal = "",
            FotoFimExpediente = dto.FotoFimExpediente,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };
    }

    public async Task<RegistrarInicioPausaResponseDto> RegisterInicioPausaAsync(Guid usuarioId, RegistrarInicioPausaRequestDto dto)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var novoPonto = new Ponto
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            TipoPonto = TipoPonto.Pausa,
            InicioPausa = DateTime.Now,
            LatitudeInicioPausa = string.IsNullOrWhiteSpace(dto.Latitude) ? 0 : Convert.ToDouble(dto.Latitude.Replace(",", ".")),
            LongitudeInicioPausa = string.IsNullOrWhiteSpace(dto.Longitude) ? 0 : Convert.ToDouble(dto.Longitude.Replace(",", ".")),
            ObservacaoInicioPausa = dto.Observacoes,
            Ativo = true
        };

        await _pontoRepository.CriarPontoAsync(novoPonto);

        return new RegistrarInicioPausaResponseDto
        {
            PontoId = novoPonto.Id,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Observacoes = dto.Observacoes,
            DataHoraInicioPausa = DateTime.Now
        };
    }

    public async Task<RegistrarFimPausaResponseDto> RegisterFimPausaAsync(Guid usuarioId, Guid pontoId, RegistrarFimPausaRequestDto dto)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var pontoPausa = await _pontoRepository.ObterPontoPorIdAsync(pontoId);
        if (pontoPausa == null || pontoPausa.TipoPonto != TipoPonto.Pausa)
            throw new InvalidOperationException("Ponto de pausa não encontrado.");

        if (pontoPausa.InicioPausa == null)
            throw new InvalidOperationException("Início de pausa não registrado.");

        pontoPausa.RetornoPausa = DateTime.Now;
        pontoPausa.LatitudeRetornoPausa = string.IsNullOrWhiteSpace(dto.Latitude) ? 0 : Convert.ToDouble(dto.Latitude.Replace(",", "."));
        pontoPausa.LongitudeRetornoPausa = string.IsNullOrWhiteSpace(dto.Longitude) ? 0 : Convert.ToDouble(dto.Longitude.Replace(",", "."));
        pontoPausa.ObservacaoFimPausa = dto.Observacoes;
        pontoPausa.Ativo = true;

        await _pontoRepository.AtualizarPontoAsync(pontoPausa.Id, pontoPausa);

        return new RegistrarFimPausaResponseDto
        {
            Latitude = dto.Latitude,  
            Longitude = dto.Longitude,  
            Observacoes = pontoPausa.ObservacaoFimPausa,
            DataHoraRetornoPausa = DateTime.Now
        };

    }


    public async Task<List<TecnicoResponseDto>> GetPontosComTrajetosAsync()
    {
        var pontos = await _pontoRepository.ObterTodosPontosAsync();
        var pontosAgrupadosPorUsuario = pontos.GroupBy(p => p.UsuarioId);

        var responseDtos = new List<TecnicoResponseDto>();

        foreach (var grupoUsuario in pontosAgrupadosPorUsuario)
        {
            var tecnico = await _usuarioRepository.ObterUsuarioPorIdAsync(grupoUsuario.Key);
            if (tecnico == null) continue;

            var tecnicoResponseDto = MapearParaTecnicoResponseDto(tecnico);
            var trajetos = grupoUsuario
               .Where(p => p.TipoPonto == TipoPonto.Expediente &&
            !string.IsNullOrWhiteSpace(p.LatitudeInicioExpediente) &&
            !string.IsNullOrWhiteSpace(p.LongitudeInicioExpediente) &&
            !string.IsNullOrWhiteSpace(p.LatitudeFimExpediente) &&
            !string.IsNullOrWhiteSpace(p.LongitudeFimExpediente))


               .Select(p => new TrajetoResponseDto
               {
                   PontoInicioId = p.Id,
                   LatitudeInicio = p.LatitudeInicioExpediente?.ToString() ?? "0",
                   LongitudeInicio = p.LongitudeInicioExpediente?.ToString() ?? "0",
                   LatitudeFim = p.LatitudeFimExpediente?.ToString() ?? "0",
                   LongitudeFim = p.LongitudeFimExpediente?.ToString() ?? "0"
               })
                .ToList();

            // Mapeia os pontos
            var pontosDto = grupoUsuario.Select(p => new ConsultarPontoResponseDto
            {
                Id = p.Id,
                InicioExpediente = p.InicioExpediente,
                FimExpediente = p.FimExpediente,
                InicioPausa = p.InicioPausa,
                RetornoPausa = p.RetornoPausa,
                HorasTrabalhadas = p.HorasTrabalhadas,
                HorasExtras = p.HorasExtras,
                HorasDevidas = p.HorasDevidas,
                LatitudeInicioExpediente = p.LatitudeInicioExpediente?.ToString() ?? "0",
                LongitudeInicioExpediente = p.LongitudeInicioExpediente?.ToString() ?? "0",
                DataHoraInicioExpediente = p.InicioExpediente,
                LatitudeFimExpediente = p.LatitudeFimExpediente?.ToString() ?? "0",
                LongitudeFimExpediente = p.LongitudeFimExpediente?.ToString() ?? "0",
                DataHoraFimExpediente = p.FimExpediente,
                LatitudeInicioPausa = p.LatitudeInicioPausa?.ToString() ?? "0",
                LongitudeInicioPausa = p.LongitudeInicioPausa?.ToString() ?? "0",
                DataHoraInicioPausa = p.InicioPausa,
                LatitudeRetornoPausa = p.LatitudeRetornoPausa?.ToString() ?? "0",
                LongitudeRetornoPausa = p.LongitudeRetornoPausa?.ToString() ?? "0",
                DataHoraRetornoPausa = p.RetornoPausa,
                UsuarioId = p.UsuarioId,
                NomeTecnico = tecnicoResponseDto.Nome,
                FotoInicioExpediente = p.FotoInicioExpediente,
                FotoFimExpediente = p.FotoFimExpediente,
                TipoPonto = p.TipoPonto,
                DistanciaPercorrida = p.DistanciaPercorrida,
                ObservacaoInicioExpediente = p.ObservacaoInicioExpediente,
                ObservacaoFimExpediente = p.ObservacaoFimExpediente,
                ObservacaoInicioPausa = p.ObservacaoInicioPausa,
                ObservacaoFimPausa = p.ObservacaoFimPausa
            }).ToList();

            tecnicoResponseDto.Pontos = pontosDto;
            tecnicoResponseDto.Trajetos = trajetos;

            responseDtos.Add(tecnicoResponseDto);
        }

        return responseDtos;
    }


    public async Task<IEnumerable<Ponto>> GetAllAsync()
    {
        return await _pontoRepository.ObterPontosPorTecnicoIdAsync(Guid.Empty);
    }

    public async Task<Ponto?> GetByIdAsync(Guid id)
    {
        return await _pontoRepository.ObterPontoPorIdAsync(id);
    }

    public async Task AddAsync(Ponto entity)
    {
        await _pontoRepository.CriarPontoAsync(entity);
    }

    public async Task UpdateAsync(Ponto entity)
    {
        await _pontoRepository.AtualizarPontoAsync(entity.Id, entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _pontoRepository.DesativarPontoAsync(id);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var ponto = await _pontoRepository.ObterPontoPorIdAsync(id);
        return ponto != null;
    }

    public async Task<ConsultarPontoResponseDto> GetPontoByIdAsync(Guid usuarioId)
    {
        var pontos = await _pontoRepository.ObterPontoPorUsuarioId(usuarioId);

        if (pontos == null || !pontos.Any())
            throw new InvalidOperationException("Nenhum ponto encontrado para este usuário.");

        var ponto = pontos.First();

        return new ConsultarPontoResponseDto
        {
            Id = ponto.Id,
            InicioExpediente = ponto.InicioExpediente,
            FimExpediente = ponto.FimExpediente,
            InicioPausa = ponto.InicioPausa,
            RetornoPausa = ponto.RetornoPausa,
            HorasTrabalhadas = ponto.HorasTrabalhadas,
            HorasExtras = ponto.HorasExtras,
            HorasDevidas = ponto.HorasDevidas,
            LatitudeInicioExpediente = ponto.LatitudeInicioExpediente?.ToString() ?? "0",
            LongitudeInicioExpediente = ponto.LongitudeInicioExpediente?.ToString() ?? "0",
            LatitudeFimExpediente = ponto.LatitudeFimExpediente?.ToString() ?? "0",
            LongitudeFimExpediente = ponto.LongitudeFimExpediente?.ToString() ?? "0",
            LatitudeInicioPausa = ponto.LatitudeInicioPausa?.ToString() ?? "0",
            LongitudeInicioPausa = ponto.LongitudeInicioPausa?.ToString() ?? "0",
            LatitudeRetornoPausa = ponto.LatitudeRetornoPausa?.ToString() ?? "0",
            LongitudeRetornoPausa = ponto.LongitudeRetornoPausa?.ToString() ?? "0",
            UsuarioId = ponto.UsuarioId,
            NomeTecnico = ponto.Tecnico?.Nome,
            Observacoes = ponto.Observacoes,
            FotoInicioExpediente = ponto.FotoInicioExpediente,
            FotoFimExpediente = ponto.FotoFimExpediente,
            TipoPonto = ponto.TipoPonto
        };
    }


    public async Task<ConsultarTecnicoResponseDto> GetTecnicoComPontosAsync(Guid usuarioId)
    {
        var tecnico = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId) as Tecnico;
        if (tecnico == null)
            throw new InvalidOperationException("Técnico não encontrado.");

        var fotoUrlFinal = !string.IsNullOrEmpty(tecnico.FotoUrl) ? tecnico.FotoUrl : "Foto não encontrada";

        var pontos = await _pontoRepository.ObterPontoPorUsuarioId(usuarioId);

        var trajetos = new List<TrajetoResponseDto>();

        foreach (var ponto in pontos)
        {
            if (ponto.TipoPonto == TipoPonto.Expediente &&
    !string.IsNullOrWhiteSpace(ponto.LatitudeInicioExpediente) &&
    !string.IsNullOrWhiteSpace(ponto.LongitudeInicioExpediente) &&
    !string.IsNullOrWhiteSpace(ponto.LatitudeFimExpediente) &&
    !string.IsNullOrWhiteSpace(ponto.LongitudeFimExpediente))

            {
                trajetos.Add(new TrajetoResponseDto
                {
                    PontoInicioId = ponto.Id,
                    LatitudeInicio = ponto.LatitudeInicioExpediente?.ToString() ?? "0",
                    LongitudeInicio = ponto.LongitudeInicioExpediente?.ToString() ?? "0",
                    LatitudeFim = ponto.LatitudeFimExpediente?.ToString() ?? "0",
                    LongitudeFim = ponto.LongitudeFimExpediente?.ToString() ?? "0"
                });

            }
        }

        return new ConsultarTecnicoResponseDto
        {
            UsuarioId = tecnico.UsuarioId,
            Nome = tecnico.Nome,
            Email = tecnico.Email,
            UserName = tecnico.UserName,
            Role = tecnico.Role,
            Ativo = tecnico.Ativo,
            Cpf = tecnico.Cpf,
            FotoUrl = fotoUrlFinal,
            HoraEntrada = tecnico.HoraEntrada,
            HoraSaida = tecnico.HoraSaida,
            HoraAlmocoInicio = tecnico.HoraAlmocoInicio,
            HoraAlmocoFim = tecnico.HoraAlmocoFim,
            IsOnline = tecnico.IsOnline,
            Pontos = pontos.Select(p => new ConsultarPontoResponseDto
            {
                Id = p.Id,
                InicioExpediente = p.InicioExpediente,
                FimExpediente = p.FimExpediente,
                InicioPausa = p.InicioPausa,
                RetornoPausa = p.RetornoPausa,
                HorasTrabalhadas = p.HorasTrabalhadas,
                HorasExtras = p.HorasExtras,
                HorasDevidas = p.HorasDevidas,

                LatitudeInicioExpediente = p.LatitudeInicioExpediente?.ToString() ?? "0",
                LongitudeInicioExpediente = p.LongitudeInicioExpediente?.ToString() ?? "0",
                DataHoraInicioExpediente = p.InicioExpediente,

                LatitudeFimExpediente = p.LatitudeFimExpediente?.ToString() ?? "0",
                LongitudeFimExpediente = p.LongitudeFimExpediente?.ToString() ?? "0",
                DataHoraFimExpediente = p.FimExpediente,

                LatitudeInicioPausa = p.LatitudeInicioPausa?.ToString() ?? "0",
                LongitudeInicioPausa = p.LongitudeInicioPausa?.ToString() ?? "0",
                DataHoraInicioPausa = p.InicioPausa,

                LatitudeRetornoPausa = p.LatitudeRetornoPausa?.ToString() ?? "0",
                LongitudeRetornoPausa = p.LongitudeRetornoPausa?.ToString() ?? "0",
                DataHoraRetornoPausa = p.RetornoPausa,

                UsuarioId = p.UsuarioId,
                NomeTecnico = tecnico.Nome,
                FotoInicioExpediente = p.FotoInicioExpediente,
                FotoFimExpediente = p.FotoFimExpediente,
                TipoPonto = p.TipoPonto,

                DistanciaPercorrida = p.DistanciaPercorrida,
                ObservacaoInicioExpediente = p.ObservacaoInicioExpediente,
                ObservacaoFimExpediente = p.ObservacaoFimExpediente,
                ObservacaoInicioPausa = p.ObservacaoInicioPausa,
                ObservacaoFimPausa = p.ObservacaoFimPausa
            }).ToList(),

            Trajetos = trajetos,

        };
    }

    Task<IEnumerable<UsuarioResponseDto>> IBaseService<Ponto>.GetAllAsync()
    {
        throw new NotImplementedException();
    }

    private TecnicoResponseDto MapearParaTecnicoResponseDto(Usuario usuario)
    {
        var tecnico = usuario as Tecnico;

        return new TecnicoResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            Nome = usuario.Nome,
            Email = usuario.Email,
            UserName = usuario.UserName,
            Role = usuario.Role,
            Ativo = usuario.Ativo,
            FotoUrl = usuario.FotoUrl,
            TipoUsuario = usuario.TipoUsuario,
            Cpf = tecnico?.Cpf ?? "N/A",
            HoraEntrada = tecnico?.HoraEntrada ?? TimeSpan.Zero,
            HoraSaida = tecnico?.HoraSaida ?? TimeSpan.Zero,
            HoraAlmocoInicio = tecnico?.HoraAlmocoInicio ?? TimeSpan.Zero,
            HoraAlmocoFim = tecnico?.HoraAlmocoFim ?? TimeSpan.Zero,
            IsOnline = tecnico?.IsOnline ?? false,
            Pontos = new List<ConsultarPontoResponseDto>(),
            Trajetos = new List<TrajetoResponseDto>()
        };
    }

    public async Task<List<PontoCombinadoResponseDto>> GetAllPontosCombinadoAsync()
    {
        var pontos = await _pontoRepository.ObterTodosPontosAsync();

        // Agrupar os pontos por usuário
        var pontosCombinados = pontos
            .GroupBy(p => p.UsuarioId)
            .SelectMany(grupo =>
            {
                // Separe os expedientes e pausas desse usuário
                var expedientes = grupo.Where(x => x.TipoPonto == TipoPonto.Expediente).ToList();
                var pausas = grupo.Where(x => x.TipoPonto == TipoPonto.Pausa).ToList();

                var lista = new List<PontoCombinadoResponseDto>();

                // 1) Se tiver AMBOS (Expediente e Pausa), vamos criar um "combo" para cada par
                if (expedientes.Any() && pausas.Any())
                {
                    foreach (var exp in expedientes)
                    {
                        foreach (var pau in pausas)
                        {
                            lista.Add(new PontoCombinadoResponseDto
                            {
                                Id = exp.Id,
                                TipoPonto = TipoPonto.Expediente,
                                UsuarioId = grupo.Key,
                                Nome = exp.Tecnico?.Nome ?? pau.Tecnico?.Nome ?? "N/A",

                                // Dados do expediente
                                InicioExpediente = exp.InicioExpediente,
                                FimExpediente = exp.FimExpediente,
                                LatitudeInicioExpediente = double.TryParse(exp.LatitudeInicioExpediente?.Replace(",", "."),
                                           out double latInicio) ? latInicio : 0,

                                LongitudeInicioExpediente = double.TryParse(exp.LongitudeInicioExpediente?.Replace(",", "."),
                                            out double longInicio) ? longInicio : 0,

                                LatitudeFimExpediente = double.TryParse(exp.LatitudeFimExpediente?.Replace(",", "."),
                                        out double latFim) ? latFim : 0,

                                LongitudeFimExpediente = double.TryParse(exp.LongitudeFimExpediente?.Replace(",", "."),
                                         out double longFim) ? longFim : 0,

                                FotoInicioExpediente = exp.FotoInicioExpediente,
                                FotoFimExpediente = exp.FotoFimExpediente,

                                // Dados da pausa
                                InicioPausa = pau.InicioPausa,
                                RetornoPausa = pau.RetornoPausa,
                                LatitudeInicioPausa = pau.LatitudeInicioPausa,
                                LongitudeInicioPausa = pau.LongitudeInicioPausa,
                                LatitudeRetornoPausa = pau.LatitudeRetornoPausa,
                                LongitudeRetornoPausa = pau.LongitudeRetornoPausa,

                                // Horas combinadas (trazendo as do expediente)
                                HorasTrabalhadas = exp.HorasTrabalhadas,
                                HorasExtras = exp.HorasExtras,
                                HorasDevidas = exp.HorasDevidas,
                                // Observações: une as do expediente e as da pausa
                                Observacoes = string.Join(" | ", new[]
                                {
                                    exp.ObservacaoInicioExpediente,
                                    exp.ObservacaoFimExpediente,
                                    pau.ObservacaoInicioPausa,
                                    pau.ObservacaoFimPausa
                                }.Where(o => !string.IsNullOrEmpty(o)))
                            });
                        }
                    }
                }
                else
                {
                    // 2) Se tiver APENAS Expedientes (nenhuma Pausa)
                    if (expedientes.Any() && !pausas.Any())
                    {
                        foreach (var exp in expedientes)
                        {
                            lista.Add(new PontoCombinadoResponseDto
                            {
                                Id = exp.Id,
                                TipoPonto = TipoPonto.Expediente,
                                UsuarioId = grupo.Key,
                                Nome = exp.Tecnico?.Nome ?? "N/A",

                                // Dados do expediente
                                InicioExpediente = exp.InicioExpediente,
                                FimExpediente = exp.FimExpediente,
                                LatitudeInicioExpediente = double.TryParse(exp.LatitudeInicioExpediente?.Replace(",", "."),
                                           out double latInicio) ? latInicio : 0,

                                LongitudeInicioExpediente = double.TryParse(exp.LongitudeInicioExpediente?.Replace(",", "."),
                                            out double longInicio) ? longInicio : 0,

                                LatitudeFimExpediente = double.TryParse(exp.LatitudeFimExpediente?.Replace(",", "."),
                                        out double latFim) ? latFim : 0,

                                LongitudeFimExpediente = double.TryParse(exp.LongitudeFimExpediente?.Replace(",", "."),
                                         out double longFim) ? longFim : 0,

                                FotoInicioExpediente = exp.FotoInicioExpediente,
                                FotoFimExpediente = exp.FotoFimExpediente,

                                // Horas combinadas (do expediente)
                                HorasTrabalhadas = exp.HorasTrabalhadas,
                                HorasExtras = exp.HorasExtras,
                                HorasDevidas = exp.HorasDevidas,
                                // Observações
                                Observacoes = string.Join(" | ", new[]
                                {
                                    exp.ObservacaoInicioExpediente,
                                    exp.ObservacaoFimExpediente
                                }.Where(o => !string.IsNullOrEmpty(o)))
                            });
                        }
                    }

                    // 3) Se tiver APENAS Pausas (nenhum Expediente)
                    if (!expedientes.Any() && pausas.Any())
                    {
                        foreach (var pau in pausas)
                        {
                            lista.Add(new PontoCombinadoResponseDto
                            {
                                Id = pau.Id,
                                TipoPonto = TipoPonto.Pausa,
                                UsuarioId = grupo.Key,
                                Nome = pau.Tecnico?.Nome ?? "N/A",

                                // Dados da pausa
                                InicioPausa = pau.InicioPausa,
                                RetornoPausa = pau.RetornoPausa,
                                LatitudeInicioPausa = pau.LatitudeInicioPausa,
                                LongitudeInicioPausa = pau.LongitudeInicioPausa,
                                LatitudeRetornoPausa = pau.LatitudeRetornoPausa,
                                LongitudeRetornoPausa = pau.LongitudeRetornoPausa,

                                HorasTrabalhadas = pau.HorasTrabalhadas,
                                HorasExtras = pau.HorasExtras,
                                HorasDevidas = pau.HorasDevidas,
                                // Observações
                                Observacoes = string.Join(" | ", new[]
                                {
                                    pau.ObservacaoInicioPausa,
                                    pau.ObservacaoFimPausa
                                }.Where(o => !string.IsNullOrEmpty(o)))
                            });
                        }
                    }
                }

                return lista;
            })
            .ToList();

        return pontosCombinados;
    }
    public async Task<List<PontoCombinadoResponseDto>> GetPontosCombinadoPorUsuarioIdAsync(Guid usuarioId)
    {
        var pontos = await _pontoRepository.ObterPontoPorUsuarioId(usuarioId);

        var expedientes = pontos.Where(x => x.TipoPonto == TipoPonto.Expediente).ToList();
        var pausas = pontos.Where(x => x.TipoPonto == TipoPonto.Pausa).ToList();

        var lista = new List<PontoCombinadoResponseDto>();

        if (expedientes.Any() && pausas.Any())
        {
            foreach (var exp in expedientes)
            {
                foreach (var pau in pausas)
                {
                    lista.Add(new PontoCombinadoResponseDto
                    {
                        PontoIdExpediente = exp.Id,
                        PontoIdPausa = pau.Id,
                        TipoPonto = TipoPonto.Expediente,
                        UsuarioId = usuarioId,
                        Nome = exp.Tecnico?.Nome ?? pau.Tecnico?.Nome ?? "N/A",

                        InicioExpediente = exp.InicioExpediente,
                        FimExpediente = exp.FimExpediente,
                        LatitudeInicioExpediente = Convert.ToDouble(exp.LatitudeInicioExpediente?.Replace(",", ".") ?? "0"),
                        LongitudeInicioExpediente = Convert.ToDouble(exp.LongitudeInicioExpediente?.Replace(",", ".") ?? "0"),
                        LatitudeFimExpediente = Convert.ToDouble(exp.LatitudeFimExpediente?.Replace(",", ".") ?? "0"),
                        LongitudeFimExpediente = Convert.ToDouble(exp.LongitudeFimExpediente?.Replace(",", ".") ?? "0"),
                        FotoInicioExpediente = exp.FotoInicioExpediente,
                        FotoFimExpediente = exp.FotoFimExpediente,
                        InicioPausa = pau.InicioPausa,
                        RetornoPausa = pau.RetornoPausa,
                        LatitudeInicioPausa = pau.LatitudeInicioPausa,
                        LongitudeInicioPausa = pau.LongitudeInicioPausa,
                        LatitudeRetornoPausa = pau.LatitudeRetornoPausa,
                        LongitudeRetornoPausa = pau.LongitudeRetornoPausa,
                        HorasTrabalhadas = exp.HorasTrabalhadas,
                        HorasExtras = exp.HorasExtras,
                        HorasDevidas = exp.HorasDevidas,
                        Observacoes = string.Join(" | ", new[] { exp.ObservacaoInicioExpediente, exp.ObservacaoFimExpediente, pau.ObservacaoInicioPausa, pau.ObservacaoFimPausa }.Where(o => !string.IsNullOrEmpty(o)))
                    });
                }
            }
        }
        else if (expedientes.Any())
        {
            foreach (var exp in expedientes)
            {
                lista.Add(new PontoCombinadoResponseDto
                {
                    PontoIdExpediente = exp.Id,
                    TipoPonto = TipoPonto.Expediente,
                    UsuarioId = usuarioId,
                    Nome = exp.Tecnico?.Nome ?? "N/A",
                    InicioExpediente = exp.InicioExpediente,
                    FimExpediente = exp.FimExpediente,
                    LatitudeInicioExpediente = Convert.ToDouble(exp.LatitudeInicioExpediente?.Replace(",", ".") ?? "0"),
                    LongitudeInicioExpediente = Convert.ToDouble(exp.LongitudeInicioExpediente?.Replace(",", ".") ?? "0"),
                    LatitudeFimExpediente = Convert.ToDouble(exp.LatitudeFimExpediente?.Replace(",", ".") ?? "0"),
                    LongitudeFimExpediente = Convert.ToDouble(exp.LongitudeFimExpediente?.Replace(",", ".") ?? "0"),
                    FotoInicioExpediente = exp.FotoInicioExpediente,
                    FotoFimExpediente = exp.FotoFimExpediente,
                    HorasTrabalhadas = exp.HorasTrabalhadas,
                    HorasExtras = exp.HorasExtras,
                    HorasDevidas = exp.HorasDevidas,
                    Observacoes = string.Join(" | ", new[] { exp.ObservacaoInicioExpediente, exp.ObservacaoFimExpediente }.Where(o => !string.IsNullOrEmpty(o)))
                });
            }
        }
        else if (pausas.Any())
        {
            foreach (var pau in pausas)
            {
                lista.Add(new PontoCombinadoResponseDto
                {
                    PontoIdPausa = pau.Id,
                    TipoPonto = TipoPonto.Pausa,
                    UsuarioId = usuarioId,
                    Nome = pau.Tecnico?.Nome ?? "N/A",
                    InicioPausa = pau.InicioPausa,
                    RetornoPausa = pau.RetornoPausa,
                    LatitudeInicioPausa = pau.LatitudeInicioPausa,
                    LongitudeInicioPausa = pau.LongitudeInicioPausa,
                    LatitudeRetornoPausa = pau.LatitudeRetornoPausa,
                    LongitudeRetornoPausa = pau.LongitudeRetornoPausa,
                    HorasTrabalhadas = pau.HorasTrabalhadas,
                    HorasExtras = pau.HorasExtras,
                    HorasDevidas = pau.HorasDevidas,
                    Observacoes = string.Join(" | ", new[] { pau.ObservacaoInicioPausa, pau.ObservacaoFimPausa }.Where(o => !string.IsNullOrEmpty(o)))
                });
            }
        }

        return lista;
    }
    public async Task<bool> VerificarExpedienteDoDiaAsync(Guid usuarioId)
    {
        var hoje = DateTime.Now.Date;
        var pontos = await _pontoRepository.ObterPontoPorUsuarioId(usuarioId);

        // Verifica se há algum registro de início de expediente no dia atual
        return pontos.Any(p => p.InicioExpediente.HasValue && p.InicioExpediente.Value.Date == hoje);
    }



}








