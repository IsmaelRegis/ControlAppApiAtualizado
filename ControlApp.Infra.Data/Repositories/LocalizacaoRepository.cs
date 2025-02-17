using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Repositories
{

    public class LocalizacaoRepository : ILocalizacaoRepository
    {
        private readonly DataContext _context;

        public LocalizacaoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Localizacao?> GetByIdAsync(Guid id)
        {
            return await _context.Localizacoes.FindAsync(id);
        }

        public async Task<IEnumerable<Localizacao>> ObterLocalizacoesPorTrajetoIdAsync(Guid trajetoId)
        {
            return await _context.Localizacoes
                .Where(l => l.TrajetoId == trajetoId)
                .ToListAsync();
        }

    }
}