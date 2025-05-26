using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly DataContext _context;

    public AuditoriaRepository(DataContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(Auditoria auditoria)
    {
        _context.Auditorias.Add(auditoria);
        await _context.SaveChangesAsync();
    }

    public IQueryable<Auditoria> Query()
    {
        return _context.Auditorias.AsNoTracking();
    }
}
