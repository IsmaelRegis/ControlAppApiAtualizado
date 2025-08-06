using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Domain.Interfaces.Services;
using System.Globalization;

public class PontoService : IPontoService
{
    private readonly IPontoRepository _pontoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IImageService _imageService;

    #region Construtor
    public PontoService(
        IPontoRepository pontoRepository,
        IUsuarioRepository usuarioRepository,
        IImageService imageService)
    {
        _pontoRepository = pontoRepository;
        _usuarioRepository = usuarioRepository;
        _imageService = imageService;
    }
    #endregion

    #region Métodos de Registro e Consulta
    public async Task<RegistrarInicioExpedienteResponseDto> RegisterInicioExpedienteAsync(Guid usuarioId, RegistrarInicioExpedienteRequestDto dto)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        // Verifica se já tem expediente hoje pra evitar duplicação
        var pontosHoje = await _pontoRepository.ObterPontosPorUsuarioEPeriodoAsync(
            usuarioId,
            DateTime.Now.Date,
            DateTime.Now.Date.AddDays(1));
        if (pontosHoje.Any(p => p.TipoPonto == TipoPonto.Expediente))
            throw new InvalidOperationException("Já existe um ponto de expediente registrado para hoje para este usuário.");

        // Salva a foto do início, se tiver
        if (dto.FotoInicioExpedienteFile != null)
        {
            var fotoInicioUrl = await _imageService.UploadImageAsync(dto.FotoInicioExpedienteFile);
            dto.FotoInicioExpediente = fotoInicioUrl;
        }

        // Cria o novo ponto de expediente
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
        var fimReal = DateTime.Now;
        if (fimReal <= ponto.InicioExpediente)
            throw new InvalidOperationException("O horário de fim do expediente não pode ser anterior ou igual ao início.");

        if (dto.FotoFimExpedienteFile != null)
        {
            var fotoFimUrl = await _imageService.UploadImageAsync(dto.FotoFimExpedienteFile);
            dto.FotoFimExpediente = fotoFimUrl;
        }

        var pausasDoExpediente = await _pontoRepository.ObterPausasPorExpedienteIdAsync(ponto.Id);

        var ultimoEvento = pausasDoExpediente.OrderByDescending(p => p.RetornoPausa)
                                              .FirstOrDefault()?.RetornoPausa
                                              ?? ponto.InicioExpediente.Value;

        var tempoTrabalhadoNesteSegmento = fimReal - ultimoEvento;

        var horasTrabalhadasFinal = ponto.HorasTrabalhadas + tempoTrabalhadoNesteSegmento;

        var tecnico = usuario as Tecnico;
        if (tecnico == null)
            throw new InvalidOperationException("Usuário não é um Técnico.");
        var jornadaEfetiva = (tecnico.HoraSaida - tecnico.HoraEntrada) - (tecnico.HoraAlmocoFim - tecnico.HoraAlmocoInicio);
        var horasExtras = horasTrabalhadasFinal > jornadaEfetiva ? horasTrabalhadasFinal - jornadaEfetiva : TimeSpan.Zero;
        var horasDevidas = horasTrabalhadasFinal < jornadaEfetiva ? jornadaEfetiva - horasTrabalhadasFinal : TimeSpan.Zero;

        ponto.FimExpediente = fimReal;
        ponto.HorasTrabalhadas = horasTrabalhadasFinal;
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
            HorasTrabalhadas = horasTrabalhadasFinal,
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

        var expediente = await _pontoRepository.ObterPontoPorIdAsync(dto.ExpedienteId);
        if (expediente == null || expediente.UsuarioId != usuarioId || expediente.TipoPonto != TipoPonto.Expediente)
            throw new InvalidOperationException("Expediente informado é inválido ou não pertence ao usuário.");

        if (!expediente.InicioExpediente.HasValue || expediente.FimExpediente.HasValue)
            throw new InvalidOperationException("O expediente já foi finalizado ou ainda não foi iniciado.");

        var pausasDoExpediente = await _pontoRepository.ObterPausasPorExpedienteIdAsync(expediente.Id);
        if (pausasDoExpediente.Any(p => p.RetornoPausa == null))
        {
            throw new InvalidOperationException("Já existe uma pausa em aberto para esse expediente.");
        }

        var ultimoEvento = pausasDoExpediente.OrderByDescending(p => p.RetornoPausa)
                                              .FirstOrDefault()?.RetornoPausa
                                              ?? expediente.InicioExpediente.Value;

        var tempoTrabalhadoNesteSegmento = DateTime.Now - ultimoEvento;

        expediente.HorasTrabalhadas += tempoTrabalhadoNesteSegmento;

        await _pontoRepository.AtualizarPontoAsync(expediente.Id, expediente);

        var novoPontoPausa = new Ponto
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            TipoPonto = TipoPonto.Pausa,
            ExpedienteId = expediente.Id,
            InicioPausa = DateTime.Now,
            LatitudeInicioPausa = string.IsNullOrWhiteSpace(dto.Latitude) ? 0 : Convert.ToDouble(dto.Latitude.Replace(",", ".")),
            LongitudeInicioPausa = string.IsNullOrWhiteSpace(dto.Longitude) ? 0 : Convert.ToDouble(dto.Longitude.Replace(",", ".")),
            ObservacaoInicioPausa = dto.Observacoes,
            Ativo = true
        };
        await _pontoRepository.CriarPontoAsync(novoPontoPausa);

        return new RegistrarInicioPausaResponseDto
        {
            PontoId = novoPontoPausa.Id,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Observacoes = dto.Observacoes,
            DataHoraInicioPausa = novoPontoPausa.InicioPausa.Value,
            HorasTrabalhadas = expediente.HorasTrabalhadas
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

        // Confere se a pausa já foi iniciada
        if (pontoPausa.InicioPausa == null)
            throw new InvalidOperationException("Início de pausa não registrado.");

        // Atualiza o ponto com os dados de retorno
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

    public async Task<TempoTrabalhadoAtualResponseDto> ObterTempoTrabalhadoAtualAsync(Guid usuarioId)
    {
        // 1. Encontra o expediente de hoje que ainda não foi finalizado
        var hoje = DateTime.Now.Date;
        var expedienteAberto = (await _pontoRepository.ObterPontosPorUsuarioEPeriodoAsync(usuarioId, hoje, hoje.AddDays(1)))
            .FirstOrDefault(p => p.TipoPonto == TipoPonto.Expediente && p.FimExpediente == null);

        // 2. Se não houver expediente iniciado, retorna o status correspondente
        if (expedienteAberto == null || !expedienteAberto.InicioExpediente.HasValue)
        {
            return new TempoTrabalhadoAtualResponseDto
            {
                Status = "Expediente não iniciado",
                TempoTrabalhadoLiquido = TimeSpan.Zero
            };
        }

        // 3. Calcula o tempo bruto desde o início do expediente até agora
        var tempoBrutoTotal = DateTime.Now - expedienteAberto.InicioExpediente.Value;
        var tempoTotalDePausas = TimeSpan.Zero;

        // 4. Busca todas as pausas associadas a este expediente
        var pausasDoExpediente = await _pontoRepository.ObterPausasPorExpedienteIdAsync(expedienteAberto.Id);

        Ponto pausaAberta = null;

        foreach (var pausa in pausasDoExpediente)
        {
            if (pausa.InicioPausa.HasValue && pausa.RetornoPausa.HasValue)
            {
                // Se a pausa já foi finalizada, soma sua duração total
                tempoTotalDePausas += pausa.RetornoPausa.Value - pausa.InicioPausa.Value;
            }
            else if (pausa.InicioPausa.HasValue && !pausa.RetornoPausa.HasValue)
            {
                // Se a pausa está em andamento, calcula o tempo decorrido até agora
                tempoTotalDePausas += DateTime.Now - pausa.InicioPausa.Value;
                pausaAberta = pausa;
            }
        }

        // 5. Calcula o tempo líquido
        var tempoLiquido = tempoBrutoTotal - tempoTotalDePausas;

        // Garante que o tempo não seja negativo
        if (tempoLiquido < TimeSpan.Zero)
        {
            tempoLiquido = TimeSpan.Zero;
        }

        // 6. Define o status final e retorna a resposta
        return new TempoTrabalhadoAtualResponseDto
        {
            Status = pausaAberta != null ? "Em pausa" : "Em expediente",
            TempoTrabalhadoLiquido = tempoLiquido,
            ExpedienteId = expedienteAberto.Id,
            PausaAtualId = pausaAberta?.Id
        };
    }

    public async Task<List<TecnicoResponseDto>> GetPontosComTrajetosAsync()
    {
        var pontos = await _pontoRepository.ObterTodosPontosAsync(); // Pega todos os pontos
        var pontosAgrupadosPorUsuario = pontos.GroupBy(p => p.UsuarioId); // Agrupa por usuário

        var responseDtos = new List<TecnicoResponseDto>();

        // Monta uma lista com técnicos e seus pontos/trajetos
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
        return await _pontoRepository.ObterPontosPorTecnicoIdAsync(Guid.Empty); // Retorna todos os pontos (simples)
    }

    public async Task<Ponto?> GetByIdAsync(Guid id)
    {
        return await _pontoRepository.ObterPontoPorIdAsync(id); // Busca um ponto pelo ID
    }

    public async Task AddAsync(Ponto entity)
    {
        await _pontoRepository.CriarPontoAsync(entity); // Adiciona um novo ponto
    }

    public async Task<RelatorioDiarioResponseDto> GetRelatorioDiarioPorUsuarioAsync(Guid usuarioId, DateTime? data = null)
    {
        var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        var dataAlvo = data?.Date ?? DateTime.Now.Date;

        var pontosDoDia = await _pontoRepository.ObterPontosPorUsuarioNaDataAsync(usuarioId, dataAlvo);

        if (pontosDoDia == null || !pontosDoDia.Any())
        {
            return null;
        }

        var expediente = pontosDoDia.FirstOrDefault(p => p.TipoPonto == TipoPonto.Expediente);
        if (expediente == null)
        {
            return null;
        }

        var pausas = pontosDoDia.Where(p => p.TipoPonto == TipoPonto.Pausa).ToList();

        var relatorio = new RelatorioDiarioResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            NomeTecnico = usuario.Nome,
            Data = dataAlvo,
            PontoExpedienteId = expediente.Id,
            InicioExpediente = expediente.InicioExpediente,
            FimExpediente = expediente.FimExpediente,
            HorasTrabalhadas = expediente.HorasTrabalhadas,
            FotoInicioExpediente = expediente.FotoInicioExpediente,
            FotoFimExpediente = expediente.FotoFimExpediente,
            HorasExtras = expediente.HorasExtras,
            HorasDevidas = expediente.HorasDevidas,
            Observacoes = string.Join(" | ", new[] { expediente.ObservacaoInicioExpediente, expediente.ObservacaoFimExpediente }.Where(o => !string.IsNullOrEmpty(o))),
            LatitudeInicioExpediente = expediente.LatitudeInicioExpediente,
            LongitudeInicioExpediente = expediente.LongitudeInicioExpediente,
            LatitudeFimExpediente = expediente.LatitudeFimExpediente,
            LongitudeFimExpediente = expediente.LongitudeFimExpediente
        };

        foreach (var pausa in pausas)
        {
            relatorio.Pausas.Add(new PausaInfoResponseDto
            {
                PontoId = pausa.Id,
                InicioPausa = pausa.InicioPausa,
                RetornoPausa = pausa.RetornoPausa,
                Duracao = (pausa.RetornoPausa.HasValue && pausa.InicioPausa.HasValue)
                          ? pausa.RetornoPausa.Value - pausa.InicioPausa.Value
                          : TimeSpan.Zero
            });
        }

        return relatorio;
    }

    public async Task UpdateAsync(Ponto entity)
    {
        await _pontoRepository.AtualizarPontoAsync(entity.Id, entity); // Atualiza um ponto existente
    }

    public async Task DeleteAsync(Guid id)
    {
        await _pontoRepository.DesativarPontoAsync(id); // Desativa um ponto
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var ponto = await _pontoRepository.ObterPontoPorIdAsync(id); // Verifica se o ponto existe
        return ponto != null;
    }

    public async Task<ConsultarPontoResponseDto> GetPontoByIdAsync(Guid usuarioId)
    {
        var pontos = await _pontoRepository.ObterPontoPorUsuarioId(usuarioId); // Busca pontos do usuário
        if (pontos == null || !pontos.Any())
            throw new InvalidOperationException("Nenhum ponto encontrado para este usuário.");

        var ponto = pontos.First(); // Pega o primeiro ponto encontrado

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

        var pontos = await _pontoRepository.ObterPontoPorUsuarioId(usuarioId); // Busca os pontos do técnico

        // Monta os trajetos com base nos pontos de expediente
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
            FotoUrl = !string.IsNullOrEmpty(tecnico.FotoUrl) ? tecnico.FotoUrl : "Foto não encontrada",
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
            Trajetos = trajetos
        };
    }

    Task<IEnumerable<UsuarioResponseDto>> IBaseService<Ponto>.GetAllAsync()
    {
        throw new NotImplementedException(); // Método não implementado
    }

    private TecnicoResponseDto MapearParaTecnicoResponseDto(Usuario usuario)
    {
        var tecnico = usuario as Tecnico; // Converte o usuário pra técnico

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
        var pontos = await _pontoRepository.ObterTodosPontosAsync(); // Pega todos os pontos
        var pontosCombinados = new List<PontoCombinadoResponseDto>();

        var gruposPorUsuario = pontos.GroupBy(p => p.UsuarioId);

        foreach (var grupo in gruposPorUsuario)
        {
            var expedientes = grupo.Where(x => x.TipoPonto == TipoPonto.Expediente).ToList();
            var pausas = grupo.Where(x => x.TipoPonto == TipoPonto.Pausa).ToList();

            // Associa cada expediente com suas próprias pausas (ExpedienteId == Id)
            foreach (var exp in expedientes)
            {
                var pausasDoExpediente = pausas.Where(p => p.ExpedienteId == exp.Id).ToList();

                if (pausasDoExpediente.Any())
                {
                    foreach (var pau in pausasDoExpediente)
                    {
                        pontosCombinados.Add(new PontoCombinadoResponseDto
                        {
                            PontoIdExpediente = exp.Id,
                            PontoIdPausa = pau.Id,
                            TipoPonto = TipoPonto.Expediente,
                            UsuarioId = grupo.Key,
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
                else
                {
                    pontosCombinados.Add(new PontoCombinadoResponseDto
                    {
                        PontoIdExpediente = exp.Id,
                        TipoPonto = TipoPonto.Expediente,
                        UsuarioId = grupo.Key,
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

            // Pausas sem expediente associado
            var pausasSemExpediente = pausas
                .Where(p => p.ExpedienteId == null || !expedientes.Any(e => e.Id == p.ExpedienteId))
                .ToList();

            foreach (var pau in pausasSemExpediente)
            {
                pontosCombinados.Add(new PontoCombinadoResponseDto
                {
                    PontoIdPausa = pau.Id,
                    TipoPonto = TipoPonto.Pausa,
                    UsuarioId = grupo.Key,
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

        return pontosCombinados;
    }

    public async Task<List<PontoCombinadoResponseDto>> GetPontosCombinadoPorUsuarioIdAsync(Guid usuarioId)
    {
        var pontos = await _pontoRepository.ObterPontoPorUsuarioId(usuarioId); // Pega os pontos do usuário
        var expedientes = pontos.Where(x => x.TipoPonto == TipoPonto.Expediente).ToList();
        var pausas = pontos.Where(x => x.TipoPonto == TipoPonto.Pausa).ToList();
        var lista = new List<PontoCombinadoResponseDto>();

        // Combina expedientes e pausas ou usa só um deles
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
        return pontos.Any(p => p.InicioExpediente.HasValue && p.InicioExpediente.Value.Date == hoje);
    }
    #endregion

    private ConsultarPontoResponseDto MapearPontoParaDto(Ponto ponto, string nomeTecnico)
    {
        return new ConsultarPontoResponseDto
        {
            Id = ponto.Id,
            UsuarioId = ponto.UsuarioId,
            NomeTecnico = nomeTecnico,
            TipoPonto = ponto.TipoPonto,

            // Dados de Expediente
            InicioExpediente = ponto.InicioExpediente,
            FimExpediente = ponto.FimExpediente,
            DataHoraInicioExpediente = ponto.InicioExpediente,
            DataHoraFimExpediente = ponto.FimExpediente,
            ObservacaoInicioExpediente = ponto.ObservacaoInicioExpediente,
            ObservacaoFimExpediente = ponto.ObservacaoFimExpediente,
            LatitudeInicioExpediente = ponto.LatitudeInicioExpediente ?? "0",
            LongitudeInicioExpediente = ponto.LongitudeInicioExpediente ?? "0",
            LatitudeFimExpediente = ponto.LatitudeFimExpediente ?? "0",
            LongitudeFimExpediente = ponto.LongitudeFimExpediente ?? "0",
            FotoInicioExpediente = ponto.FotoInicioExpediente,
            FotoFimExpediente = ponto.FotoFimExpediente,

            // Dados de Pausa (COM A CORREÇÃO)
            InicioPausa = ponto.InicioPausa,
            RetornoPausa = ponto.RetornoPausa,
            DataHoraInicioPausa = ponto.InicioPausa,
            DataHoraRetornoPausa = ponto.RetornoPausa,
            ObservacaoInicioPausa = ponto.ObservacaoInicioPausa,
            ObservacaoFimPausa = ponto.ObservacaoFimPausa,
            LatitudeInicioPausa = ponto.LatitudeInicioPausa.HasValue ? ponto.LatitudeInicioPausa.Value.ToString(CultureInfo.InvariantCulture) : "0",
            LongitudeInicioPausa = ponto.LongitudeInicioPausa.HasValue ? ponto.LongitudeInicioPausa.Value.ToString(CultureInfo.InvariantCulture) : "0",
            LatitudeRetornoPausa = ponto.LatitudeRetornoPausa.HasValue ? ponto.LatitudeRetornoPausa.Value.ToString(CultureInfo.InvariantCulture) : "0",
            LongitudeRetornoPausa = ponto.LongitudeRetornoPausa.HasValue ? ponto.LongitudeRetornoPausa.Value.ToString(CultureInfo.InvariantCulture) : "0",

            // Dados Calculados
            HorasTrabalhadas = ponto.HorasTrabalhadas,
            HorasExtras = ponto.HorasExtras,
            HorasDevidas = ponto.HorasDevidas,
            DistanciaPercorrida = ponto.DistanciaPercorrida
        };
    }


}