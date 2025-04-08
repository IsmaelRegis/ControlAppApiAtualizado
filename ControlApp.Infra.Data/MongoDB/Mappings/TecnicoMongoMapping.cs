/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ControlApp.Infra.Data.MongoDB.Mappings
{
    public class TecnicoMongoMapping : UsuarioMongoMapping
    {
        [BsonElement("cpf")]
        public string? Cpf { get; set; }

        [BsonElement("horaEntrada")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan HoraEntrada { get; set; }

        [BsonElement("horaSaida")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan HoraSaida { get; set; }

        [BsonElement("horaAlmocoInicio")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan HoraAlmocoInicio { get; set; }

        [BsonElement("horaAlmocoFim")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan HoraAlmocoFim { get; set; }

        [BsonElement("isOnline")]
        public bool IsOnline { get; set; } = false;

        [BsonElement("latitudeAtual")]
        public string? LatitudeAtual { get; set; }

        [BsonElement("longitudeAtual")]
        public string? LongitutdeAtual { get; set; }

        [BsonElement("numeroMatricula")]
        public string? NumeroMatricula { get; set; }

        [BsonElement("empresaId")]
        [BsonRepresentation(BsonType.String)]
        public Guid? EmpresaId { get; set; }

        [BsonElement("nomeDaEmpresa")]
        public string? NomeDaEmpresa { get; set; }
    }
}*/
