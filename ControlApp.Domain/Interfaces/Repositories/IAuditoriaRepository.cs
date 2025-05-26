public interface IAuditoriaRepository
{
    Task RegistrarAsync(Auditoria auditoria);
    IQueryable<Auditoria> Query();
}
