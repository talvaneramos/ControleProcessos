

namespace ControleProcessos.Models
{
    public class Processo
    {
        public int Id { get; set; }

        public required string NumeroProcesso { get; set; }

        public required string DescricaoObjeto { get; set; }

        public string? Especificacao { get; set; }

        public int QtdVolume { get; set; }

        public string? LocalEntrada { get; set; }
        
        public DateTime DataEntrada { get; set; } = DateTime.UtcNow; 
        
        public int TempoDias => (DateTime.UtcNow - DataEntrada).Days;

        public string? LocalAtual { get; set; }
        public string? ResponsavelAtual { get; set; }
    }
}
