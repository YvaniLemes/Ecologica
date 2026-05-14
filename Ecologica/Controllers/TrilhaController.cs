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

        // Lista todas as etapas da trilha
        public IActionResult Index()
        {
            // O código de reset FOI REMOVIDO daqui para o progresso salvar de verdade!
            var listaDaTrilha = _context.trilha_progresso.ToList();
            ViewBag.ArvoresVidas = 5; 
            return View(listaDaTrilha);
        }

        // Abre a página de conteúdo da lição
        public IActionResult Detalhes(int id)
        {
            var etapa = _context.trilha_progresso.FirstOrDefault(t => t.Id == id);

            if (etapa == null || etapa.EstaBloqueado)
            {
                return RedirectToAction("Index");
            }

            return View(etapa);
        }

        // AÇÃO PARA CONCLUIR: Faz o progresso avançar e desbloqueia a próxima fase
        [HttpPost]
        public IActionResult ConcluirEtapa(int id)
        {
            var etapaAtual = _context.trilha_progresso.FirstOrDefault(t => t.Id == id);
            
            if (etapaAtual != null)
            {
                etapaAtual.Progresso = 1.0; // Define como concluída

                var proximaEtapa = _context.trilha_progresso.FirstOrDefault(t => t.Id == id + 1);
                
                if (proximaEtapa != null)
                {
                    proximaEtapa.EstaBloqueado = false; // Desbloqueia a próxima (ela virará broto!)
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}