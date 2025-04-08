/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ControlApp.Infra.Data.MongoDB.Mappings
{
    public class TrajetoMongoMapping
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("data")]
        public DateTime Data { get; set; }

        [BsonElement("usuarioId")]
        [BsonRepresentation(BsonType.String)]
        public Guid UsuarioId { get; set; }

        [BsonElement("localizacoes")]
        public List<LocalizacaoMongoMapping> Localizacoes { get; set; } = new List<LocalizacaoMongoMapping>();

        [BsonElement("distanciaTotalKm")]
        public double DistanciaTotalKm { get; set; }

        [BsonElement("duracaoTotal")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan DuracaoTotal { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Em andamento";
    }
}
*/