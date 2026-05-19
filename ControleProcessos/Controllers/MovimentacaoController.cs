using ControleProcessos.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ControleProcessos.DTOs;

namespace ControleProcessos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimentacaoController : ControllerBase
    {
        private readonly IMovimentacaoService _movimentacaoService;

        public MovimentacaoController(IMovimentacaoService movimentacaoService)
        {
            _movimentacaoService = movimentacaoService;
        }

        [HttpPost]
        public IActionResult Movimentar([FromBody] MovimentacaoRequest request)
        {
            try
            {
                _movimentacaoService.Movimentar(
                    request.ProcessoId,
                    request.NovoLocal,
                    request.Responsavel,
                    request.Justificativa
                );

                return Ok(new { mensagem = "Movimentação registrada com sucesso" });
            }
            catch (Exception ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("processo/{processoId}")]
        public IActionResult ObterHistorico(int processoId)
        {
            var historico = _movimentacaoService.ObterHistoricoPorProcesso(processoId);

            if (!historico.Any())
                return NotFound(new { mensagem = "Nenhum histórico encontrado." });

            return Ok(historico);
        }

        [HttpGet("exportar/excel/{id}")]
        public IActionResult ExportarExcel(int id)
        {
            var file = _movimentacaoService.ExportarExcel(id);

            return File(file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "historico.xlsx");
        }

        [HttpGet("exportar/pdf/{id}")]
        public IActionResult ExportarPdf(int id)
        {
            var file = _movimentacaoService.ExportarPdf(id);

            return File(file,
                "application/pdf",
                "historico.pdf");
        }
    }
}

