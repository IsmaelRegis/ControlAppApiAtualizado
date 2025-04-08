/*using System;
using ControlApp.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ControlApp.Infra.Data.MongoDB.Mappings
{
    public class PontoMongoMapping
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("inicioExpediente")]
        public DateTime? InicioExpediente { get; set; }

        [BsonElement("fimExpediente")]
        public DateTime? FimExpediente { get; set; }

        [BsonElement("inicioPausa")]
        public DateTime? InicioPausa { get; set; }

        [BsonElement("retornoPausa")]
        public DateTime? RetornoPausa { get; set; }

        [BsonElement("horasTrabalhadas")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan HorasTrabalhadas { get; set; }

        [BsonElement("horasExtras")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan HorasExtras { get; set; }

        [BsonElement("horasDevidas")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan HorasDevidas { get; set; }

        [BsonElement("latitudeInicioExpediente")]
        public string? LatitudeInicioExpediente { get; set; }

        [BsonElement("longitudeInicioExpediente")]
        public string? LongitudeInicioExpediente { get; set; }

        [BsonElement("latitudeInicioPausa")]
        public double? LatitudeInicioPausa { get; set; }

        [BsonElement("longitudeInicioPausa")]
        public double? LongitudeInicioPausa { get; set; }

        [BsonElement("latitudeFimExpediente")]
        public string? LatitudeFimExpediente { get; set; }

        [BsonElement("longitudeFimExpediente")]
        public string? LongitudeFimExpediente { get; set; }

        [BsonElement("latitudeRetornoPausa")]
        public double? LatitudeRetornoPausa { get; set; }

        [BsonElement("longitudeRetornoPausa")]
        public double? LongitudeRetornoPausa { get; set; }

        [BsonElement("observacoes")]
        public string? Observacoes { get; set; }

        [BsonElement("tipoPonto")]
        public TipoPonto TipoPonto { get; set; }

        [BsonElement("usuarioId")]
        [BsonRepresentation(BsonType.String)]
        public Guid UsuarioId { get; set; }

        [BsonElement("ativo")]
        public bool Ativo { get; set; } = true;

        [BsonElement("fotoInicioExpediente")]
        public string? FotoInicioExpediente { get; set; }

        [BsonElement("fotoFimExpediente")]
        public string? FotoFimExpediente { get; set; }

        [BsonElement("distanciaPercorrida")]
        public string? DistanciaPercorrida { get; set; }

        [BsonElement("observacaoInicioExpediente")]
        public string? ObservacaoInicioExpediente { get; set; }

        [BsonElement("observacaoFimExpediente")]
        public string? ObservacaoFimExpediente { get; set; }

        [BsonElement("observacaoInicioPausa")]
        public string? ObservacaoInicioPausa { get; set; }

        [BsonElement("observacaoFimPausa")]
        public string? ObservacaoFimPausa { get; set; }
    }
}*/