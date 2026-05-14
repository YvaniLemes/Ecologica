using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecologica.Data;
using Ecologica.Models;      // Para encontrar os modelos 
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecologica.Controllers
{
    public class RegistrosCarbonoController : Controller
    {
        // Substitua 'SeuDbContext' pelo nome real do seu arquivo de contexto do banco de dados
        private readonly ApplicationDbContext _context;

        public RegistrosCarbonoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Este é o método que carrega a página inicial do histórico (Index.cshtml)
        public async Task<IActionResult> Index()
        {
            // 1. Busca os registros do banco para a tabela
            var registros = await _context.RegistrosCarbono
                .Include(r => r.AtividadeRelacionada)
                .ToListAsync();

            // 2. Prepara os dados do Quiz (Enviando para a ViewBag)
            ViewBag.PerguntasQuiz = new List<QuizViewModel>
            {
                new QuizViewModel {
                    Pergunta = "Qual transporte emite menos CO2 por km?",
                    Opcoes = new List<string> { "Carro Individual", "Ônibus Elétrico", "Bicicleta" },
                    RespostaCorretaIndice = 2,
                    Explicacao = "A bicicleta tem emissão zero de gases poluentes!"
                },
                new QuizViewModel {
                    Pergunta = "Qual destas fontes é considerada 'Energia Limpa'?",
                    Opcoes = new List<string> { "Carvão Mineral", "Energia Solar", "Gás Natural" },
                    RespostaCorretaIndice = 1,
                    Explicacao = "A energia solar é renovável e não emite CO2 na geração."
                }
            };

            // 3. Prepara os dados do Gráfico (Exemplos para teste)
            ViewBag.LabelsGrafico = new[] { "Transporte", "Energia", "Alimentação" };
            ViewBag.ValoresGrafico = new[] { 45, 25, 30 };
            
            // Nome do usuário para a saudação
            ViewBag.NomeUsuario = "Yvani";

            // Retorna a lista de registros para a Model da View
            return View(registros);
        }

        // Métodos Create, Edit, Delete viriam abaixo...
    }
}