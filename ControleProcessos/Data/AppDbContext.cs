using Microsoft.EntityFrameworkCore;
using ControleProcessos.Models;

namespace ControleProcessos.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<Processo> Processos { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
}