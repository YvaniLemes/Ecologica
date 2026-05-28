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

        // 1. PAINEL PRINCIPAL (DASHBOARD COM GRÁFICO E QUIZ ATRELADO)
        public IActionResult Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var nomeLogado = HttpContext.Session.GetString("UsuarioNome");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            ViewBag.NomeUsuario = nomeLogado ?? "Usuário";

            // --- LÓGICA DO QUIZ ---
            var todasPerguntas = ObterListaPerguntasCompleta();
            var questoesFeitasTags = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId.Value && c.Nome.StartsWith("Quiz-Ok-"))
                .Select(c => c.Nome)
                .ToList();

            ViewBag.TotalMedalhas = _context.Conquistas
                .Count(c => c.UsuarioId == usuarioId.Value && !c.Nome.StartsWith("Quiz-Ok-"));

            int totalMedalhas = ViewBag.TotalMedalhas;
            int metaFinal = 12;
            int progresso = (int)((Math.Min(totalMedalhas, metaFinal) / (double)metaFinal) * 100);
            ViewBag.ProgressoBarra = progresso;

            bool temEcoIniciante = _context.Conquistas.Any(c => c.UsuarioId == usuarioId.Value && c.Nome == "Eco-Iniciante 🎯");
            bool temGuardiao = _context.Conquistas.Any(c => c.UsuarioId == usuarioId.Value && c.Nome == "Guardião Verde 🍀");
            bool temMestre = _context.Conquistas.Any(c => c.UsuarioId == usuarioId.Value && c.Nome == "Mestre Sustentável 🌎");

            string ranking = "Iniciante 🌱";
            if (temMestre) ranking = "Mestre Sustentável 🌎";
            else if (temGuardiao) ranking = "Guardião Verde 🍀";
            else if (temEcoIniciante) ranking = "Eco Aprendiz 🌱";

            ViewBag.RankingUsuario = ranking;

            ViewBag.TotalAcertosQuiz = _context.Conquistas
                .Count(c => c.UsuarioId == usuarioId.Value && c.Nome.StartsWith("Quiz-Ok-"));

            var perguntasDisponiveis = todasPerguntas
                .Where(p => !questoesFeitasTags.Contains($"Quiz-Ok-{p.Id}"))
                .ToList();

            if (!perguntasDisponiveis.Any()) perguntasDisponiveis = todasPerguntas;

            int quantidadePerguntas = 3;
            if (temEcoIniciante) quantidadePerguntas = 4;
            if (temGuardiao) quantidadePerguntas = 5;
            if (temMestre) quantidadePerguntas = 6;

            ViewBag.PerguntasQuiz = perguntasDisponiveis
                .OrderBy(p => Guid.NewGuid())
                .Take(quantidadePerguntas)
                .ToList();

            // --- FIM DA LÓGICA DO QUIZ ---

            // --- DADOS PARA O GRÁFICO (REVISADO) ---
            var registros = _context.RegistrosCarbono
                .Where(r => r.IdUsuario == usuarioId)
                .Include(r => r.AtividadeRelacionada)
                .ToList();

            var dadosGrafico = registros
                .GroupBy(r => string.IsNullOrEmpty(r.AtividadeRelacionada?.Nome) ? "Outros" : r.AtividadeRelacionada.Nome)
                .Select(g => new
                {
                    Nome = g.Key,
                    Total = g.Sum(r => r.EmissaoTotal ?? 0.0)
                }).ToList();

            ViewBag.DadosGrafico = new
            {
                labels = dadosGrafico.Select(d => d.Nome).ToList(),
                valores = dadosGrafico.Select(d => d.Total).ToList()
            };

            ViewBag.Conquistas = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId.Value && !c.Nome.StartsWith("Quiz-Ok-"))
                .ToList();

            return View(registros);
        }

        // ACTION AJAX: Quiz
        [HttpPost]
        public IActionResult ValidarRespostaQuiz(int questaoIndice, int alternativaEscolhida)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return Json(new { success = false, message = "Usuário não logado." });
            }

            try
            {
                var listaPerguntas = ObterListaPerguntasCompleta();
                var questao = listaPerguntas.FirstOrDefault(p => p.Id == questaoIndice);

                if (questao == null)
                {
                    return BadRequest();
                }

                bool acertou = (alternativaEscolhida == questao.RespostaCorretaIndice);

                if (acertou)
                {
                    string tagConquistaQuiz = $"Quiz-Ok-{questao.Id}";

                    bool jaMarcou = _context.Conquistas
                        .Any(c => c.UsuarioId == usuarioId.Value && c.Nome == tagConquistaQuiz);

                    if (!jaMarcou)
                    {
                        _context.Conquistas.Add(new Conquista
                        {
                            Nome = tagConquistaQuiz,
                            Descricao = "Resposta correta registrada internamente.",
                            UsuarioId = usuarioId.Value,
                            DataAquisicao = DateTime.Now
                        });
                        _context.SaveChanges();
                    }

                    int totalAcertos = _context.Conquistas
                        .Count(c => c.UsuarioId == usuarioId.Value && c.Nome.StartsWith("Quiz-Ok-"));

                    string[] medalhas = { "Eco Aprendiz 🌱", "Guardião Verde 🍀", "Mestre Sustentável 🌎" };
                    int[] metas = { 3, 7, 12 };

                    for (int i = 0; i < medalhas.Length; i++)
                    {
                        if (totalAcertos >= metas[i] && !_context.Conquistas.Any(c => c.UsuarioId == usuarioId.Value && c.Nome == medalhas[i]))
                        {
                            _context.Conquistas.Add(new Conquista
                            {
                                Nome = medalhas[i],
                                Descricao = "Parabéns por atingir esta marca!",
                                UsuarioId = usuarioId.Value,
                                DataAquisicao = DateTime.Now
                            });
                        }
                    }

                    _context.SaveChanges();
                }

                return Json(new
                {
                    correto = acertou,
                    explicacao = questao.Explicacao
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao processar a resposta: " + ex.Message });
            }
        }

        // Perguntas do Quiz
        private List<QuizViewModel> ObterListaPerguntasCompleta()
        {
            return new List<QuizViewModel>
    {
        new QuizViewModel { Id = 0, Pergunta = "Qual transporte emite menos CO2 por km/passageiro?", Opcoes = new List<string> { "Carro Individual", "Ônibus Elétrico", "Bicicleta" }, RespostaCorretaIndice = 2, Explicacao = "A bicicleta tem emissão zero de gases poluentes durante o trajeto! 🍀" },
        new QuizViewModel { Id = 1, Pergunta = "Qual destas práticas ajuda a reduzir o desperdício de energia?", Opcoes = new List<string> { "Deixar a luz acesa", "Usar lâmpadas LED", "Manter a geladeira aberta" }, RespostaCorretaIndice = 1, Explicacao = "Lâmpadas LED consomem muito menos energia e duram mais! 💡" },
        new QuizViewModel { Id = 2, Pergunta = "O que é a pegada de carbono?", Opcoes = new List<string> { "Marca de sapato", "Medida das emissões de gases estufa", "Quantidade de lixo reciclado" }, RespostaCorretaIndice = 1, Explicacao = "A pegada de carbono calcula o impacto ambiental das nossas ações! 🌍" },
        new QuizViewModel { Id = 3, Pergunta = "Qual é o principal benefício da reciclagem?", Opcoes = new List<string> { "Economizar energia", "Aumentar o lixo", "Nada acontece" }, RespostaCorretaIndice = 0, Explicacao = "Reciclar economiza recursos naturais e reduz o consumo de energia! ♻️" },
        new QuizViewModel { Id = 4, Pergunta = "Como podemos economizar água no banho?", Opcoes = new List<string> { "Tomar banhos de 30 min", "Fechar o registro ao se ensaboar", "Deixar a torneira aberta" }, RespostaCorretaIndice = 1, Explicacao = "Fechar o registro economiza muitos litros de água! 💧" }
    };
        }

        [HttpPost]
        public IActionResult ReiniciarConquistas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            var conquistasUsuario = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId.Value)
                .ToList();

            _context.Conquistas.RemoveRange(conquistasUsuario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 2. HISTÓRICO
        public IActionResult Historico()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            ViewBag.NomeUsuario = HttpContext.Session.GetString("UsuarioNome") ?? "Usuário";

            if (usuarioId == null) return RedirectToAction("Login", "Usuario");

            var registros = _context.RegistrosCarbono
                .Where(r => r.IdUsuario == usuarioId)
                .Include(r => r.AtividadeRelacionada)
                .OrderByDescending(r => r.DataRegistro)
                .ToList();

            return View(registros);
        }

        // 3. INCLUIR (GET)
        public IActionResult Create()
        {
            ViewBag.NomeUsuario = HttpContext.Session.GetString("UsuarioNome") ?? "Usuário";
            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View();
        }

        // 4. INCLUIR (POST)
        [HttpPost]
        public IActionResult Create(RegistroCarbonoModel novoRegistro)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var tipoAtividade = _context.Atividades.Find(novoRegistro.IdAtividade);

            if (tipoAtividade != null && usuarioId != null)
            {
                novoRegistro.IdUsuario = usuarioId.Value;
                // CORRIGIDO: tratamento de valores nulos
                novoRegistro.EmissaoTotal = (novoRegistro.Quantidade ?? 0) * (tipoAtividade.FatorEmissao ?? 0);
                novoRegistro.DataRegistro = DateTime.Now;

                _context.RegistrosCarbono.Add(novoRegistro);
                _context.SaveChanges();

                var jaTemConquista = _context.Conquistas
                    .Any(c => c.UsuarioId == usuarioId && c.Nome == "Eco-Iniciante 🎯");

                if (!jaTemConquista)
                {
                    _context.Conquistas.Add(new Conquista
                    {
                        Nome = "Eco-Iniciante 🎯",
                        Descricao = "Você realizou sua primeira ação sustentável no Ecologica!",
                        UsuarioId = usuarioId.Value,
                        DataAquisicao = DateTime.Now
                    });
                    _context.SaveChanges();
                }

                return RedirectToAction("Historico");
            }

            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View(novoRegistro);
        }

        // 5. ALTERAR (GET)
        public IActionResult Edit(int id)
        {
            ViewBag.NomeUsuario = HttpContext.Session.GetString("UsuarioNome") ?? "Usuário";
            var registro = _context.RegistrosCarbono.Find(id);
            if (registro == null) return NotFound();

            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View(registro);
        }

        // 6. ALTERAR (POST)
        [HttpPost]
        public IActionResult Edit(RegistroCarbonoModel registroEditado)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var tipoAtividade = _context.Atividades.Find(registroEditado.IdAtividade);

            if (tipoAtividade != null && usuarioId != null)
            {
                registroEditado.IdUsuario = usuarioId.Value;
                // CORRIGIDO: tratamento de valores nulos
                registroEditado.EmissaoTotal = (registroEditado.Quantidade ?? 0) * (tipoAtividade.FatorEmissao ?? 0);
                registroEditado.DataRegistro = DateTime.Now;

                _context.RegistrosCarbono.Update(registroEditado);
                _context.SaveChanges();
                return RedirectToAction("Historico");
            }

            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View(registroEditado);
        }

        [HttpPost]
        public IActionResult ResetarConquistas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            var conquistas = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId.Value)
                .ToList();

            _context.Conquistas.RemoveRange(conquistas);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 7. APAGAR
        public IActionResult Delete(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var registro = _context.RegistrosCarbono.Find(id);

            if (registro != null && registro.IdUsuario == usuarioId)
            {
                _context.RegistrosCarbono.Remove(registro);
                _context.SaveChanges();
            }

            return RedirectToAction("Historico");
        }
    }
}
