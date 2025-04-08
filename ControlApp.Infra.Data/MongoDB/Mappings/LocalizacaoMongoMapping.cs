/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ControlApp.Infra.Data.MongoDB.Mappings
{
    public class LocalizacaoMongoMapping
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid LocalizacaoId { get; set; }

        [BsonElement("latitude")]
        public double? Latitude { get; set; }

        [BsonElement("longitude")]
        public double? Longitude { get; set; }

        [BsonElement("dataHora")]
        public DateTime DataHora { get; set; }

        [BsonElement("precisao")]
        public double Precisao { get; set; }

        [BsonElement("trajetoId")]
        [BsonRepresentation(BsonType.String)]
        public Guid TrajetoId { get; set; }
    }
}

*/