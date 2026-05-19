namespace ControleProcessos.DTOs
{
    public class ProcessoImportDTO
    {
        public required string NumeroProcesso { get; set; }
        public required string Descricao { get; set; }
        public required string Especificacao { get; set; }
        public int QtdVolume { get; set; }
        public required string Local { get; set; }
        public required string Responsavel { get; set; }
    }
}
