using ControleProcessos.Models;

namespace ControleProcessos.Interfaces
{
    public interface IMovimentacaoService
    {
        void Movimentar(int processoId, string novoLocal, string responsavel, string justificativa);

        List<Movimentacao> ObterHistoricoPorProcesso(int processoId);
        byte[] ExportarExcel(int processoId);
        byte[] ExportarPdf(int processoId);
    }
}
