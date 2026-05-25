using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecologica.Models;
using Microsoft.AspNetCore.Http;
using Ecologica.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Ecologica.Controllers
{
    public class AtividadeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AtividadeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var nomeLogado = HttpContext.Session.GetString("UsuarioNome");

            if (usuarioId == null) return RedirectToAction("Login", "Usuario");

            ViewBag.NomeUsuario = nomeLogado ?? "Usuário";

            var todasPerguntas = ObterListaPerguntasCompleta();
            var questoesFeitasTags = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId.Value && c.Nome.StartsWith("Quiz-Ok-"))
                .Select(c => c.Nome)
                .ToList();

            var perguntasDisponiveis = todasPerguntas
                .Where(p => !questoesFeitasTags.Contains($"Quiz-Ok-{p.Id}"))
                .ToList();

            if (!perguntasDisponiveis.Any()) perguntasDisponiveis = todasPerguntas;

            ViewBag.PerguntasQuiz = perguntasDisponiveis.Take(3).ToList();

            var dadosGrafico = _context.RegistrosCarbono
                .Where(r => r.IdUsuario == usuarioId)
                .Select(r => new { NomeAtividade = r.AtividadeRelacionada != null ? r.AtividadeRelacionada.Nome : "Sem Atividade", Emissao = r.EmissaoTotal })
                .GroupBy(x => x.NomeAtividade)
                .Select(g => new { Nome = g.Key ?? "Outros", Total = g.Sum(x => x.Emissao) })
                .ToList();

            ViewBag.LabelsGrafico = dadosGrafico.Select(d => d.Nome).ToArray();
            ViewBag.ValoresGrafico = dadosGrafico.Select(d => d.Total).ToArray();
            ViewBag.Conquistas = _context.Conquistas.Where(c => c.UsuarioId == usuarioId.Value && !c.Nome.StartsWith("Quiz-Ok-")).ToList();

            var registros = _context.RegistrosCarbono.Where(r => r.IdUsuario == usuarioId).Include(r => r.AtividadeRelacionada).ToList();
            return View(registros);
        }

        [HttpPost]
        public IActionResult ValidarRespostaQuiz(int questaoIndice, int alternativaEscolhida)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return Json(new { success = false, message = "Usuário não logado." });

            var listaPerguntas = ObterListaPerguntasCompleta();
            var questao = listaPerguntas.FirstOrDefault(p => p.Id == questaoIndice);
            if (questao == null) return BadRequest();

            bool acertou = (alternativaEscolhida == questao.RespostaCorretaIndice);

            if (acertou)
            {
                string tagConquistaQuiz = $"Quiz-Ok-{questao.Id}";
                var jaMarcou = _context.Conquistas.Any(c => c.UsuarioId == usuarioId && c.Nome == tagConquistaQuiz);
                if (!jaMarcou)
                {
                    _context.Conquistas.Add(new Conquista { Nome = tagConquistaQuiz, Descricao = "Resposta correta.", UsuarioId = usuarioId.Value, DataAquisicao = DateTime.Now });
                    var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
                    if (usuario != null) usuario.Pontos += 50;
                    _context.SaveChanges();
                }

                int totalAcertos = _context.Conquistas.Count(c => c.UsuarioId == usuarioId && c.Nome.StartsWith("Quiz-Ok-"));
                if (totalAcertos >= 3 && !_context.Conquistas.Any(c => c.UsuarioId == usuarioId && c.Nome == "Mestre do Quiz 🧠"))
                {
                    _context.Conquistas.Add(new Conquista { Nome = "Mestre do Quiz 🧠", Descricao = "Acertou 3 ou mais desafios!", UsuarioId = usuarioId.Value, DataAquisicao = DateTime.Now });
                    _context.SaveChanges();
                }
            }
            return Json(new { correto = acertou, explicacao = questao.Explicacao! });
        }

        private List<QuizViewModel> ObterListaPerguntasCompleta()
        {
            return new List<QuizViewModel> {
                new QuizViewModel { Id = 0, Pergunta = "Qual transporte emite menos CO2?", Opcoes = new List<string> { "Carro", "Ônibus", "Bicicleta" }, RespostaCorretaIndice = 2, Explicacao = "Bicicleta é zero emissão!" },
                new QuizViewModel { Id = 1, Pergunta = "O que é 'Energia Limpa'?", Opcoes = new List<string> { "Carvão", "Solar/Eólica", "Pilhas" }, RespostaCorretaIndice = 1, Explicacao = "Renováveis não emitem CO2." },
                new QuizViewModel { Id = 2, Pergunta = "Tempo de decomposição da garrafa PET?", Opcoes = new List<string> { "100 anos", "450 anos", "20 anos" }, RespostaCorretaIndice = 1, Explicacao = "Plásticos levam séculos!" }
                // (Adicione o restante das suas perguntas aqui...)
            };
        }

        public IActionResult Historico()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Usuario");
            var registros = _context.RegistrosCarbono.Where(r => r.IdUsuario == usuarioId).Include(r => r.AtividadeRelacionada).OrderByDescending(r => r.DataRegistro).ToList();
            return View(registros);
        }

        public IActionResult Create()
        {
            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(RegistroCarbonoModel novoRegistro)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var tipoAtividade = _context.Atividades.Find(novoRegistro.IdAtividade);

            if (tipoAtividade != null && usuarioId != null)
            {
                novoRegistro.IdUsuario = usuarioId.Value;
                novoRegistro.EmissaoTotal = (double)(novoRegistro.Quantidade * tipoAtividade.FatorEmissao);
                novoRegistro.DataRegistro = DateTime.Now;
                _context.RegistrosCarbono.Add(novoRegistro);

                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
                if (usuario != null)
                {
                    usuario.Pontos += (novoRegistro.EmissaoTotal < 20) ? 200 : 100;

                    // Aumenta a posição e limita entre 0 e 48
                    usuario.PosicaoNoMapa = Math.Clamp(usuario.PosicaoNoMapa + 1, 0, 48);
                }

                _context.SaveChanges();
                return RedirectToAction("Mapa", "Trilha");
            }
            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View(novoRegistro);
        }

        public IActionResult Edit(int id)
        {
            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View(_context.RegistrosCarbono.Find(id));
        }

        [HttpPost]
        public IActionResult Edit(RegistroCarbonoModel registroEditado)
        {
            _context.RegistrosCarbono.Update(registroEditado);
            _context.SaveChanges();
            return RedirectToAction("Historico");
        }

        public IActionResult Delete(int id)
        {
            var registro = _context.RegistrosCarbono.Find(id);
            if (registro != null) { _context.RegistrosCarbono.Remove(registro); _context.SaveChanges(); }
            return RedirectToAction("Historico");
        }
    }
}