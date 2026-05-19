using ClosedXML.Excel;
using ControleProcessos.Data;
using ControleProcessos.DTOs;
using ControleProcessos.Interfaces;
using ControleProcessos.Models;
using System.Collections.Generic;
using System.Linq;


namespace ControleProcessos.Services;
public class ProcessoService : IProcessoService
{
    private readonly AppDbContext _context;

    public ProcessoService(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Processo> ObterTodos()
    {
        return _context.Processos.ToList();
    }

    public Processo? ObterPorId(int id)
    {
        return _context.Processos.FirstOrDefault(p => p.Id == id);
    }

    public void Adicionar(Processo processo)
    {
        processo.LocalAtual = processo.LocalEntrada;

        processo.ResponsavelAtual = string.IsNullOrWhiteSpace(processo.ResponsavelAtual)
            ? "Não informado"
            : processo.ResponsavelAtual;

        _context.Processos.Add(processo);
        _context.SaveChanges();
    }

    public bool Atualizar(Processo processo)
    {
        var existente = _context.Processos.FirstOrDefault(p => p.Id == processo.Id);

        if (existente == null)
            return false;

        existente.NumeroProcesso = processo.NumeroProcesso;
        existente.DescricaoObjeto = processo.DescricaoObjeto;
        existente.QtdVolume = processo.QtdVolume;
        existente.Especificacao = processo.Especificacao;        
        existente.LocalEntrada = processo.LocalEntrada;
        existente.LocalAtual = processo.LocalEntrada;
        existente.ResponsavelAtual = processo.ResponsavelAtual;
        

        _context.SaveChanges();
        return true;
    }

    public bool Remover(int id)
    {
        var processo = _context.Processos.FirstOrDefault(p => p.Id == id);

        if (processo == null)
            return false;

        _context.Processos.Remove(processo);
        _context.SaveChanges();
        return true;
    }

    public Processo? BuscarPorNumero(string numero)
    {
        numero = LimparNumero(numero);

        return _context.Processos
            .AsEnumerable()
            .FirstOrDefault(p => LimparNumero(p.NumeroProcesso) == numero);
    }

    private string LimparNumero(string valor)
    {
        return valor.Replace(".", "").Replace(" ", "").Trim();
    }

    public ImportacaoResultadoDTO Importar(Stream stream)
    {
        var resultado = new ImportacaoResultadoDTO();

        try
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var range = worksheet.RangeUsed();

            if (range == null)
            {
                resultado.Sucesso = false;
                resultado.Erros.Add("A planilha está vazia.");
                return resultado;
            }

            var linhas = range.RowsUsed().Skip(1);

            int linhaNumero = 2;
                        
            foreach (var linha in linhas)
            {
                string numero = linha.Cell(1).GetString().Trim();
                string descricao = linha.Cell(2).GetString().Trim();
                string especificacao = linha.Cell(3).GetString().Trim();
                int qtd = int.TryParse(linha.Cell(4).GetString(), out var valor) ? valor : 0;
                string local = linha.Cell(5).GetString().Trim();

                if (string.IsNullOrWhiteSpace(numero))
                    resultado.Erros.Add($"Linha {linhaNumero}: Número do processo obrigatório");

                if (string.IsNullOrWhiteSpace(descricao))
                    resultado.Erros.Add($"Linha {linhaNumero}: Descrição obrigatória");

                if (qtd < 0)
                    resultado.Erros.Add($"Linha {linhaNumero}: Quantidade inválida");

                if (string.IsNullOrWhiteSpace(local))
                    resultado.Erros.Add($"Linha {linhaNumero}: Local obrigatório");

                if (!string.IsNullOrWhiteSpace(numero) &&
                    _context.Processos.Any(p => p.NumeroProcesso == numero))
                {
                    resultado.Erros.Add($"Linha {linhaNumero}: Processo já existe");
                }

                linhaNumero++;
            }
                        
            if (resultado.Erros.Any())
            {
                resultado.Sucesso = false;
                return resultado;
            }
                        
            foreach (var linha in linhas)
            {
                var processo = new Processo
                {
                    NumeroProcesso = linha.Cell(1).GetString().Trim(),
                    DescricaoObjeto = linha.Cell(2).GetString().Trim(),
                    Especificacao = linha.Cell(3).GetString().Trim(),
                    QtdVolume = int.TryParse(linha.Cell(4).GetString(), out var valor) ? valor : 0,
                    LocalEntrada = linha.Cell(5).GetString().Trim(),
                    LocalAtual = linha.Cell(5).GetString().Trim(),
                    ResponsavelAtual = null,
                    DataEntrada = DateTime.Now
                };

                _context.Processos.Add(processo);
            }

            _context.SaveChanges();

            resultado.Sucesso = true;
            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Sucesso = false;
            resultado.Erros.Add("Erro interno ao processar a planilha.");
            resultado.Erros.Add(ex.Message);

            return resultado;
        }
    }
}