using ControleProcessos.Data;
using ControleProcessos.Interfaces;
using ControleProcessos.Models;

namespace ControleProcessos.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public Usuario? Login(string email, string senha)
        {
            email = email.Trim().ToLower();
            senha = senha.Trim();

            return _context.Usuarios
                .FirstOrDefault(u =>
                    u.Email.ToLower() == email && u.Senha == senha
                );
        }

        public void Cadastrar(Usuario usuario)
        {
            var existeUsuario = _context.Usuarios.Any();

            if (!existeUsuario)
            {               
                usuario.Role = "Admin";
            }
            else
            {
                usuario.Role = "User";
            }

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
        }

        public bool RedefinirSenha(string email, string novaSenha)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return false;

            usuario.Senha = novaSenha;

            _context.SaveChanges();

            return true;
        }
    }
}
