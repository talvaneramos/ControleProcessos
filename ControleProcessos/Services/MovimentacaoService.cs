using ClosedXML.Excel;
using ControleProcessos.Data;
using ControleProcessos.Interfaces;
using ControleProcessos.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ControleProcessos.Services
{
    public class MovimentacaoService : IMovimentacaoService
    {
        private readonly AppDbContext _context;

        public MovimentacaoService(AppDbContext context)
        {
            _context = context;
        }

        public void Movimentar(int processoId, string novoLocal, string responsavel, string justicativa)
        {
            var processo = _context.Processos.FirstOrDefault(p => p.Id == processoId);

            if (processo == null)
                throw new Exception("Processo não encontrado");

            var movimentacao = new Movimentacao
            {
                ProcessoId = processoId,
                LocalOrigem = string.IsNullOrEmpty(processo.LocalAtual)
                    ? "Cadastro Inicial"
                    : processo.LocalAtual,

                LocalDestino = novoLocal,
                Responsavel = responsavel,
                Justificativa = justicativa,
                DataMovimentacao = DateTime.UtcNow
            };

            processo.LocalAtual = novoLocal;
            processo.ResponsavelAtual = responsavel;

            _context.Movimentacoes.Add(movimentacao);

            _context.SaveChanges();
        }
        
        public List<Movimentacao> ObterHistoricoPorProcesso(int processoId)
        {
            return _context.Movimentacoes
                .Where(m => m.ProcessoId == processoId)
                .OrderByDescending(m => m.DataMovimentacao)
                .ToList();
        }

        public byte[] ExportarExcel(int processoId)
        {
            var dados = _context.Movimentacoes
                .Where(m => m.ProcessoId == processoId)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Historico");
                        
            worksheet.Cell(1, 1).Value = "Data";
            worksheet.Cell(1, 2).Value = "Origem";
            worksheet.Cell(1, 3).Value = "Destino";
            worksheet.Cell(1, 4).Value = "Responsável";

            int linha = 2;

            foreach (var m in dados)
            {
                worksheet.Cell(linha, 1).Value = m.DataMovimentacao;
                worksheet.Cell(linha, 2).Value = m.LocalOrigem;
                worksheet.Cell(linha, 3).Value = m.LocalDestino;
                worksheet.Cell(linha, 4).Value = m.Responsavel;
                linha++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public byte[] ExportarPdf(int processoId)
        {
            var dados = _context.Movimentacoes
                .Where(m => m.ProcessoId == processoId)
                .OrderBy(m => m.DataMovimentacao)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Column(header =>
                    {
                        header.Item().Text("CONTROLE DE PROCESSOS")
                            .FontSize(22)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        header.Item().Text($"Histórico de Movimentações - Secretária Municipal de Araruama - SESAU")
                            .FontSize(14)
                            .FontColor(Colors.Grey.Darken2);

                        header.Item().PaddingTop(5).Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingVertical(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(90);
                            columns.ConstantColumn(100);
                            columns.ConstantColumn(100);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(120);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Data");
                            header.Cell().Element(HeaderStyle).Text("Origem");
                            header.Cell().Element(HeaderStyle).Text("Destino");
                            header.Cell().Element(HeaderStyle).Text("Justificativa");
                            header.Cell().Element(HeaderStyle).Text("Responsável");

                            static IContainer HeaderStyle(IContainer container)
                            {
                                return container
                                    .BorderBottom(1)
                                    .BorderColor("#D6D6D6")
                                    .PaddingVertical(6)
                                    .PaddingHorizontal(4)
                                    .DefaultTextStyle(x => x
                                        .FontSize(10)
                                        .SemiBold()
                                        .FontColor("#666666"));
                            }
                        });

                        foreach (var m in dados)
                        {
                            table.Cell().Element(CellStyle)
                                .Text(m.DataMovimentacao
                                .ToLocalTime()
                                .ToString("dd/MM/yyyy HH:mm"));

                            table.Cell().Element(CellStyle)
                                .Text(m.LocalOrigem);

                            table.Cell().Element(CellStyle)
                                .Text(m.LocalDestino);

                            table.Cell().Element(CellStyle)
                                .Text(m.Justificativa);

                            table.Cell().Element(CellStyle)
                                .Text(m.Responsavel);
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .BorderBottom(0.5f)
                                .BorderColor("#EAEAEA")
                                .PaddingVertical(10)
                                .PaddingHorizontal(4)
                                .DefaultTextStyle(x => x
                                    .FontSize(9)
                                    .FontColor("#222222"));
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Sistema Controle de Processos • Página ");
                            text.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        }

    }
}
