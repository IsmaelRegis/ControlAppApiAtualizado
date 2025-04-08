/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

namespace ControlApp.Infra.Data.MongoDB.Configurations
{
    public class MongoDbConfig
    {
        public static void ConfigureMongoDbMappings()
        {
            // Registrar mapeamentos personalizados
            BsonClassMap.RegisterClassMap<Ponto>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                // Configurações específicas de mapeamento
            });
        }
    }
}
*/