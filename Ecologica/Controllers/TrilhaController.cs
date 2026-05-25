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

        // Método auxiliar para garantir que a trilha sempre exista
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
            GarantirDadosIniciais(); // Isso evita o erro de "vazio"
            var listaDaTrilha = _context.trilha_progresso.OrderBy(t => t.Id).ToList();
            var usuario = _context.Usuarios.FirstOrDefault();

            ViewBag.TotalXP = usuario != null ? usuario.Pontos : 0;
            ViewBag.ArvoresPlantadas = usuario != null ? usuario.QuantidadeArvores : 0;

            return View(listaDaTrilha);
        }

        public IActionResult Detalhes(int id)
        {
            var etapa = _context.trilha_progresso.FirstOrDefault(t => t.Id == id);
            if (etapa == null || etapa.EstaBloqueado) return RedirectToAction("Index");
            return View(etapa);
        }

        [HttpPost]
        public IActionResult ConcluirEtapa(int id)
        {
            var etapaAtual = _context.trilha_progresso.FirstOrDefault(t => t.Id == id);
            var usuario = _context.Usuarios.FirstOrDefault();

            if (etapaAtual != null)
            {
                if (etapaAtual.Progresso < 1.0 && usuario != null)
                {
                    usuario.Pontos += 100;
                }
                etapaAtual.Progresso = 1.0;

                if (id == 3)
                {
                    if (usuario != null) usuario.QuantidadeArvores += 1;
                    foreach (var e in _context.trilha_progresso) { e.Progresso = 0.0; e.EstaBloqueado = true; }
                    var p = _context.trilha_progresso.OrderBy(t => t.Id).FirstOrDefault();
                    if (p != null) p.EstaBloqueado = false;
                }
                else
                {
                    var proxima = _context.trilha_progresso.FirstOrDefault(t => t.Id == id + 1);
                    if (proxima != null) proxima.EstaBloqueado = false;
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Mapa()
        {
            var usuario = _context.Usuarios.FirstOrDefault();
            int posicao = usuario != null ? usuario.PosicaoNoMapa : 0;
            ViewBag.PosicaoNoMapa = Math.Clamp(posicao, 0, 48);
            ViewBag.TotalXP = usuario != null ? usuario.Pontos : 0;
            ViewBag.ArvoresPlantadas = usuario != null ? usuario.QuantidadeArvores : 0;
            return View();
        }
    }
}