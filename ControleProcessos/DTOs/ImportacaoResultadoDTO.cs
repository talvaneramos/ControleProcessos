using System.Collections.Generic;

namespace ControleProcessos.DTOs
{
    public class ImportacaoResultadoDTO
    {
        public bool Sucesso { get; set; }
        public List<string> Erros { get; set; } = new();
    }
}
