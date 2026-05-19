using ControleProcessos.Models;

namespace ControleProcessos.Interfaces
{
    public interface IUsuarioService
    {
        Usuario? Login(string email, string senha);
        void Cadastrar(Usuario usuario);
        bool RedefinirSenha(string email, string novaSenha);
    }
}
