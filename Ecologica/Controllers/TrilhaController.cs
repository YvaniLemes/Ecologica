using Microsoft.AspNetCore.Mvc;
using Ecologica.Data;
using Ecologica.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;

namespace Ecologica.Controllers
{
    public class TrilhaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrilhaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =================================================================
        // LÓGICA DA TRILHA ECOLÓGICA (Etapas, Bloqueios, XP)
        // =================================================================
        private void GarantirDadosIniciais()
        {
            if (!_context.trilha_progresso.Any())
            {
                var etapas = new List<TrilhaConhecimento>
                {
                    new TrilhaConhecimento { Id = 1, Titulo = "Introdução", EstaBloqueado = false, Progresso = 0.0 },
                    new TrilhaConhecimento { Id = 2, Titulo = "Energia", EstaBloqueado = true, Progresso = 0.0 },
                    new TrilhaConhecimento { Id = 3, Titulo = "Desafio Final", EstaBloqueado = true, Progresso = 0.0 }
                };
                _context.trilha_progresso.AddRange(etapas);
                _context.SaveChanges();
            }
        }

        public IActionResult Index()
        {
            GarantirDadosIniciais();
            var listaDaTrilha = _context.trilha_progresso.OrderBy(t => t.Id).ToList();
            var usuario = _context.Usuarios.FirstOrDefault();

            ViewBag.TotalXP = usuario != null ? usuario.Pontos : 0;
            ViewBag.ArvoresPlantadas = usuario != null ? usuario.QuantidadeArvores : 0;

            return View(listaDaTrilha);
        }

        public IActionResult Detalhes(int id)
        {
            var etapa = _context.trilha_progresso.FirstOrDefault(t => t.Id == id);
            if (etapa == null || (etapa.EstaBloqueado ?? false))
                return RedirectToAction("Index");
            return View(etapa);
        }

        [HttpPost]
        public IActionResult ConcluirEtapa(int id)
        {
            var etapaAtual = _context.trilha_progresso.FirstOrDefault(t => t.Id == id);
            var usuario = _context.Usuarios.FirstOrDefault();

            if (etapaAtual != null && usuario != null)
            {
                // Lógica de XP
                if (etapaAtual.Progresso < 1.0)
                {
                    usuario.Pontos += 100;
                }
                etapaAtual.Progresso = 1.0;

                // Lógica de Avanço da Trilha
                if (id == 3)
                {
                    usuario.QuantidadeArvores += 1;
                    foreach (var e in _context.trilha_progresso) { e.Progresso = 0.0; e.EstaBloqueado = true; }
                    var p = _context.trilha_progresso.OrderBy(t => t.Id).FirstOrDefault();
                    if (p != null) p.EstaBloqueado = false;
                }
                else
                {
                    var proxima = _context.trilha_progresso.FirstOrDefault(t => t.Id == id + 1);
                    if (proxima != null) proxima.EstaBloqueado = false;
                }

                // =================================================================
                // LÓGICA DO MAPA ECOLÓGICO (Independente)
                // Caso queira que o peão ande uma casa a cada etapa concluída:
                // =================================================================
                usuario.PosicaoNoMapa += 1;
                if (usuario.PosicaoNoMapa > 47) usuario.PosicaoNoMapa = 47;

                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // =================================================================
        // LÓGICA DO MAPA ECOLÓGICO (Visualização)
        // =================================================================
        public IActionResult Mapa()
        {
            var usuario = _context.Usuarios.FirstOrDefault();

            // Se usuario for null, posicao será 0. 
            // Se usuario.PosicaoNoMapa for null, o ?? 0 resolve.
            int posicao = usuario?.PosicaoNoMapa ?? 0;

            ViewBag.PosicaoNoMapa = Math.Clamp(posicao, 0, 48);
            ViewBag.TotalXP = usuario?.Pontos ?? 0;
            ViewBag.ArvoresPlantadas = usuario?.QuantidadeArvores ?? 0;

            return View();
        }
    }
}