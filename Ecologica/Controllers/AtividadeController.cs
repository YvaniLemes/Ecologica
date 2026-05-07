using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecologica.Models;
using Microsoft.AspNetCore.Http;
using Ecologica.Models.Data;
using System.Collections.Generic;
using System.Linq;

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

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            ViewBag.NomeUsuario = nomeLogado;

            // --- INÍCIO DA LÓGICA DO QUIZ ---
            var perguntas = new List<QuizViewModel>
            {
                new QuizViewModel {
                    Pergunta = "Qual transporte emite menos CO2 por km/passageiro?",
                    Opcoes = new List<string> { "Carro Individual", "Ônibus Elétrico", "Bicicleta" },
                    RespostaCorretaIndice = 2,
                    Explicacao = "A bicicleta tem emissão zero de gases poluentes durante o trajeto!"
                },
                new QuizViewModel {
                    Pergunta = "O que é 'Energia Limpa'?",
                    Opcoes = new List<string> { "Energia de carvão", "Energia Solar/Eólica", "Energia de pilhas comuns" },
                    RespostaCorretaIndice = 1,
                    Explicacao = "Fontes renováveis não emitem CO2 durante a geração."
                }
            };
            ViewBag.PerguntasQuiz = perguntas;
            // --- FIM DA LÓGICA DO QUIZ ---

            // 1. DADOS PARA O GRÁFICO
            var dadosGrafico = _context.RegistrosCarbono
                .Where(r => r.IdUsuario == usuarioId)
                .Select(r => new
                {
                    NomeAtividade = r.AtividadeRelacionada != null ? r.AtividadeRelacionada.Nome : "Sem Atividade",
                    Emissao = r.EmissaoTotal
                })
                .GroupBy(x => x.NomeAtividade)
                .Select(g => new
                {
                    Nome = g.Key ?? "Outros",
                    Total = g.Sum(x => x.Emissao)
                })
                .ToList();

            ViewBag.LabelsGrafico = dadosGrafico.Select(d => d.Nome).ToArray();
            ViewBag.ValoresGrafico = dadosGrafico.Select(d => d.Total).ToArray();

            ViewBag.Conquistas = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId)
                .ToList();

            var registros = _context.RegistrosCarbono
                .Where(r => r.IdUsuario == usuarioId)
                .Include(r => r.AtividadeRelacionada)
                .ToList();

            return View(registros);
        }

        public IActionResult Create()
        {
            ViewBag.NomeUsuario = HttpContext.Session.GetString("UsuarioNome");
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
                _context.SaveChanges();

                var jaTemConquista = _context.Conquistas
                    .Any(c => c.UsuarioId == usuarioId && c.Nome == "Eco-Iniciante");

                if (!jaTemConquista)
                {
                    _context.Conquistas.Add(new Conquista
                    {
                        Nome = "Eco-Iniciante",
                        Descricao = "Você realizou sua primeira ação sustentável no Ecologica!",
                        UsuarioId = usuarioId.Value,
                        DataAquisicao = DateTime.Now
                    });
                    _context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View(novoRegistro);
        }

        public IActionResult Edit(int id)
        {
            var registro = _context.RegistrosCarbono.Find(id);
            if (registro == null) return NotFound();

            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View(registro);
        }

        [HttpPost]
        public IActionResult Edit(RegistroCarbonoModel registroEditado)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var tipoAtividade = _context.Atividades.Find(registroEditado.IdAtividade);

            if (tipoAtividade != null && usuarioId != null)
            {
                registroEditado.IdUsuario = usuarioId.Value;
                registroEditado.EmissaoTotal = (double)(registroEditado.Quantidade * tipoAtividade.FatorEmissao);
                registroEditado.DataRegistro = DateTime.Now;

                _context.RegistrosCarbono.Update(registroEditado);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View(registroEditado);
        }

        public IActionResult Delete(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var registro = _context.RegistrosCarbono.Find(id);

            if (registro != null && registro.IdUsuario == usuarioId)
            {
                _context.RegistrosCarbono.Remove(registro);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}