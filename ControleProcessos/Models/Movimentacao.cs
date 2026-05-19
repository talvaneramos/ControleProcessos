namespace ControleProcessos.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }
        public int ProcessoId { get; set; }
        public string LocalOrigem { get; set; } = string.Empty;
        public string LocalDestino { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
        public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;
        public string? Justificativa { get; set; }
    }
}
