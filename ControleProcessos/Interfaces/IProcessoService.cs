using ControleProcessos.DTOs;
using ControleProcessos.Models;

namespace ControleProcessos.Interfaces
{
    public interface IProcessoService
    {
        IEnumerable<Processo> ObterTodos();
        Processo? ObterPorId(int id);
        void Adicionar(Processo processo);
        bool Atualizar(Processo processo);
        bool Remover(int id);
        Processo? BuscarPorNumero(string numero);
        ImportacaoResultadoDTO Importar(Stream stream);
        
    }
}
