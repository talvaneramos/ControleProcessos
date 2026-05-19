using ControleProcessos.Interfaces;
using ControleProcessos.Models;
using ControleProcessos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ControleProcessos.DTOs;

namespace ControleProcessos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessoController : ControllerBase
    {

        private readonly IProcessoService _service;

        public ProcessoController(IProcessoService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var processos = _service.ObterTodos();

            if (!processos.Any())
                return NoContent();

            return Ok(processos);
        }

        [HttpPost]
        public IActionResult Post(Processo processo)
        {
            _service.Adicionar(processo);

            return CreatedAtAction(
                nameof(Get),
                new { id = processo.Id },
                new
                {
                    mensagem = "Cadastrado com sucesso",
                    data = processo
                });
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Processo processo)
        {
            if (id != processo.Id)
                return BadRequest("ID inconsistente");

            var atualizado = _service.Atualizar(processo);

            if (!atualizado)
                return NotFound("Processo não encontrado");

            return Ok(new
            {
                mensagem = "Alterado com sucesso"
            });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var removido = _service.Remover(id);

            if (!removido)
                return NotFound("Processo não encontrado");

            return Ok(new
            {
                mensagem = "Registro removido com sucesso"
            });
        }

        [HttpGet("buscar")]
        public IActionResult BuscarPorNumero([FromQuery] string numero)
        {
            numero = numero.Replace(".", "").Trim();

            var processo = _service.BuscarPorNumero(numero);

            if (processo == null)
                return NotFound(new { mensagem = "Processo não encontrado" });

            return Ok(processo);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var processo = _service.ObterPorId(id);

            if (processo == null)
                return NotFound();

            return Ok(processo);
        }
        
        [HttpPost("importar")]
        public IActionResult Importar()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();

                if (file == null || file.Length == 0)
                    return BadRequest("Arquivo inválido");

                using var stream = file.OpenReadStream();

                var resultado = _service.Importar(stream);

                if (!resultado.Sucesso)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao importar",
                    detalhe = ex.Message
                });
            }
        }
    }
}
