/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ControlApp.Infra.Data.MongoDB.Mappings
{
    public class UsuarioMongoMapping
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid UsuarioId { get; set; }

        [BsonElement("nome")]
        public string? Nome { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("senha")]
        public string? Senha { get; set; }

        [BsonElement("role")]
        public UserRole Role { get; set; }

        [BsonElement("userName")]
        public string? UserName { get; set; }

        [BsonElement("ativo")]
        public bool Ativo { get; set; } = true;

        [BsonElement("tipoUsuario")]
        public string? TipoUsuario { get; set; }

        [BsonElement("fotoUrl")]
        public string? FotoUrl { get; set; }

        [BsonElement("dataHoraUltimaAutenticacao")]
        public DateTime? DataHoraUltimaAutenticacao { get; set; }
    }
}
*/