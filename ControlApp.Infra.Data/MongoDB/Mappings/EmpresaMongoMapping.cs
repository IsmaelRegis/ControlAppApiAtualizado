/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ControlApp.Infra.Data.MongoDB.Mappings
{
    public class EmpresaMongoMapping
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid EnderecoId { get; set; }

        [BsonElement("cep")]
        public string? Cep { get; set; }

        [BsonElement("logradouro")]
        public string? Logradouro { get; set; }

        [BsonElement("bairro")]
        public string? Bairro { get; set; }

        [BsonElement("cidade")]
        public string? Cidade { get; set; }

        [BsonElement("estado")]
        public string? Estado { get; set; }

        [BsonElement("complemento")]
        public string? Complemento { get; set; }

        [BsonElement("numero")]
        public string? Numero { get; set; }
    }
}
*/