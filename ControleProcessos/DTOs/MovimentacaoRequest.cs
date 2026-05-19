using ControleProcessos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleProcessos.DTOs
{
    public class MovimentacaoRequest
    {
        public int ProcessoId { get; set; }
        public string NovoLocal { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;

        public string Justificativa { get; set; } = string.Empty;
    }
}
