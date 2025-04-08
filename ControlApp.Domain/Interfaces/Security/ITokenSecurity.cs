using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Interfaces.Security
{
    public interface ITokenSecurity
    {
        string CreateToken(Guid usuarioId, string userRole, string audience = "VibeService");
    }
}
