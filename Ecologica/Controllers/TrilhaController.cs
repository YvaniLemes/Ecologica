using Microsoft.AspNetCore.Mvc;
using Ecologica.Data;
using Ecologica.Models;
using System.Linq;

namespace Ecologica.Controllers
{
    public class TrilhaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrilhaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Exibe o mapa da trilha
        public IActionResult Index()
        {
            var listaDaTrilha = _context.trilha_progresso.ToList();

            // Busca o nosso usuário no banco para ler os dados reais dele
            var usuario = _context.Usuarios.FirstOrDefault();

            // Se o usuário existir, mandamos os dados reais dele para a tela.
            // Se não existir, usamos valores padrão (0) para não quebrar a tela.
            ViewBag.TotalXP = usuario != null ? usuario.Pontos : 0;
            ViewBag.ArvoresPlantadas = usuario != null ? usuario.QuantidadeArvores : 0;

            return View(listaDaTrilha);
        }

        public IActionResult Detalhes(int id)
        {
            TrilhaConhecimento? etapa = _context.trilha_progresso.FirstOrDefault(t => t.Id == id);

            if (etapa == null || etapa.EstaBloqueado)
            {
                return RedirectToAction("Index");
            }

            return View(etapa);
        }

        // AÇÃO PRINCIPAL: Avança ou Finaliza a Trilha
        [HttpPost]
        public IActionResult ConcluirEtapa(int id)
        {
            TrilhaConhecimento? etapaAtual = _context.trilha_progresso.FirstOrDefault(t => t.Id == id);
            var usuario = _context.Usuarios.FirstOrDefault(); // Pega o jogador atual

            if (etapaAtual != null)
            {
                // Se a lição ainda não tinha sido concluída antes, dá os 100 pontos para o usuário
                if (etapaAtual.Progresso < 1.0 && usuario != null)
                {
                    usuario.Pontos += 100;
                }

                etapaAtual.Progresso = 1.0; // Conclui a lição atual

                // Se for a ÚLTIMA LIÇÃO (Desafio Final - Id 3), o ciclo se completa!
                if (id == 3)
                {
                    if (usuario != null)
                    {
                        usuario.QuantidadeArvores += 1; // 🌳 GANHOU MAIS UMA ÁRVORE PERMANENTE!
                    }

                    // RESET DO MAPA: Prepara a trilha para o "New Game Plus" (recomeçar)
                    var todasEtapas = _context.trilha_progresso.ToList();
                    foreach (var etapa in todasEtapas)
                    {
                        etapa.Progresso = 0.0;
                        etapa.EstaBloqueado = true; // Tranca tudo
                    }

                    // Destranca apenas a primeira para reiniciar o ciclo
                    var primeira = todasEtapas.OrderBy(t => t.Id).FirstOrDefault();
                    if (primeira != null) primeira.EstaBloqueado = false;
                }
                else
                {
                    // Se NÃO for a última, apenas destranca a próxima fase normalmente
                    TrilhaConhecimento? proximaEtapa = _context.trilha_progresso.FirstOrDefault(t => t.Id == id + 1);
                    if (proximaEtapa != null)
                    {
                        proximaEtapa.EstaBloqueado = false;
                    }
                }

                _context.SaveChanges(); // Salva tudo de uma vez só no SQLite!
            }

            return RedirectToAction("Index");
        }

        // METODO DO MAPA CORRIGIDO (Sem duplicidade e integrado aos pontos do Usuario)
        public IActionResult Mapa()
        {
            var usuario = _context.Usuarios.FirstOrDefault();
            int totalXP = usuario != null ? usuario.Pontos : 0;

            // Calcula a posição de 0 a 48
            int posicaoCalculada = totalXP / 100;
            ViewBag.PosicaoNoMapa = Math.Clamp(posicaoCalculada, 0, 48);

            ViewBag.TotalXP = totalXP;
            ViewBag.ArvoresPlantadas = usuario != null ? usuario.QuantidadeArvores : 0;

            return View();
        }
    }
}
