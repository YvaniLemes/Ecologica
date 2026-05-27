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

            // CORREÇÃO: Usa o operador de coalescência nula para garantir que nunca passará um valor nulo à ViewBag
            ViewBag.NomeUsuario = nomeLogado ?? "Usuário";

            // --- LÓGICA DO QUIZ CONECTADO AO USUÁRIO ---
            var todasPerguntas = ObterListaPerguntasCompleta();

            // Buscamos as tags das perguntas que o usuário já acertou usando o Id real da questão
            var questoesFeitasTags = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId.Value && c.Nome.StartsWith("Quiz-Ok-"))
                .Select(c => c.Nome)
                .ToList();

            // Quantidade de medalhas reais
            ViewBag.TotalMedalhas = _context.Conquistas
                .Count(c => c.UsuarioId == usuarioId.Value &&
                            !c.Nome.StartsWith("Quiz-Ok-"));


            // --- ADICIONE ESTA PARTE ABAIXO ---
            int totalMedalhas = ViewBag.TotalMedalhas;
            int metaFinal = 12;
            int progresso = (int)((Math.Min(totalMedalhas, metaFinal) / (double)metaFinal) * 100);
            ViewBag.ProgressoBarra = progresso;
            // ----------------------------------

            // --- Ranking ecológico visual ---
            bool temEcoIniciante = _context.Conquistas.Any(c => c.UsuarioId == usuarioId.Value && c.Nome == "Eco-Iniciante 🎯");
            bool temGuardiao = _context.Conquistas.Any(c => c.UsuarioId == usuarioId.Value && c.Nome == "Guardião Verde 🍀");
            bool temMestre = _context.Conquistas.Any(c => c.UsuarioId == usuarioId.Value && c.Nome == "Mestre Sustentável 🌎");

            string ranking = "Iniciante 🌱";
            if (temMestre) ranking = "Mestre Sustentável 🌎";
            else if (temGuardiao) ranking = "Guardião Verde 🍀";
            else if (temEcoIniciante) ranking = "Eco Aprendiz 🌱";

            ViewBag.RankingUsuario = ranking;

            // Filtramos a lista usando a nova propriedade .Id do QuizViewModel
            var perguntasDisponiveis = todasPerguntas
                .Where(p => !questoesFeitasTags.Contains($"Quiz-Ok-{p.Id}"))
                .ToList();

            // Se ele já respondeu todas com sucesso, reiniciamos o ciclo
            if (!perguntasDisponiveis.Any()) perguntasDisponiveis = todasPerguntas;

            int quantidadePerguntas = 3;

            // Usamos as variáveis definidas no topo (sem repetir o "bool")
            if (temEcoIniciante) quantidadePerguntas = 5;
            if (temGuardiao) quantidadePerguntas = 7;
            if (temMestre) quantidadePerguntas = 10;

            // Embaralha perguntas
            ViewBag.PerguntasQuiz = perguntasDisponiveis
                .OrderBy(p => Guid.NewGuid())
                .Take(quantidadePerguntas)
                .ToList();


            // --- FIM DA LÓGICA DO QUIZ ---

            // DADOS PARA O GRÁFICO
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

            // FILTRO DE CONQUISTAS: Pegamos apenas as medalhas visuais reais (ignorando as tags internas do quiz)
            ViewBag.Conquistas = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId.Value && !c.Nome.StartsWith("Quiz-Ok-"))
                .ToList();

            var registros = _context.RegistrosCarbono
                .Where(r => r.IdUsuario == usuarioId)
                .Include(r => r.AtividadeRelacionada)
                .ToList();

            return View(registros);
        }

        // ACTION AJAX: Chamada quando o usuário clica em uma alternativa do Quiz

        [HttpPost]
        public IActionResult ValidarRespostaQuiz(int questaoIndice, int alternativaEscolhida)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Usuário não logado."
                });
            }

            var listaPerguntas = ObterListaPerguntasCompleta();

            var questao = listaPerguntas
                .FirstOrDefault(p => p.Id == questaoIndice);

            if (questao == null)
            {
                return BadRequest();
            }

            bool acertou =
                (alternativaEscolhida == questao.RespostaCorretaIndice);

            if (acertou)
            {
                string tagConquistaQuiz =
                    $"Quiz-Ok-{questao.Id}";

                bool jaMarcou = _context.Conquistas
                    .Any(c =>
                        c.UsuarioId == usuarioId.Value &&
                        c.Nome == tagConquistaQuiz);

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
                    .Count(c =>
                        c.UsuarioId == usuarioId.Value &&
                        c.Nome.StartsWith("Quiz-Ok-"));

                // Medalha 1
                if (totalAcertos >= 3 &&
                    !_context.Conquistas.Any(c =>
                        c.UsuarioId == usuarioId.Value &&
                        c.Nome == "Eco Aprendiz 🌱"))
                {
                    _context.Conquistas.Add(new Conquista
                    {
                        Nome = "Eco Aprendiz 🌱",
                        Descricao = "Acertou 3 desafios ecológicos!",
                        UsuarioId = usuarioId.Value,
                        DataAquisicao = DateTime.Now
                    });
                }

                // Medalha 2
                if (totalAcertos >= 7 &&
                    !_context.Conquistas.Any(c =>
                        c.UsuarioId == usuarioId.Value &&
                        c.Nome == "Guardião Verde 🍀"))
                {
                    _context.Conquistas.Add(new Conquista
                    {
                        Nome = "Guardião Verde 🍀",
                        Descricao = "Acertou 7 desafios ecológicos!",
                        UsuarioId = usuarioId.Value,
                        DataAquisicao = DateTime.Now
                    });
                }

                // Medalha 3
                if (totalAcertos >= 12 &&
                    !_context.Conquistas.Any(c =>
                        c.UsuarioId == usuarioId.Value &&
                        c.Nome == "Mestre Sustentável 🌎"))
                {
                    _context.Conquistas.Add(new Conquista
                    {
                        Nome = "Mestre Sustentável 🌎",
                        Descricao = "Dominou o Eco-Game!",
                        UsuarioId = usuarioId.Value,
                        DataAquisicao = DateTime.Now
                    });
                }

                _context.SaveChanges();
            }

            return Json(new
            {
                correto = acertou,
                explicacao = questao.Explicacao
            });
        }


        // Método auxiliar para centralizar as perguntas do seu Eco-Game (Expandido para 15 perguntas)
        private List<QuizViewModel> ObterListaPerguntasCompleta()
        {
            return new List<QuizViewModel>
            {
                new QuizViewModel { Id = 0, Pergunta = "Qual transporte emite menos CO2 por km/passageiro?", Opcoes = new List<string> { "Carro Individual", "Ônibus Elétrico", "Bicicleta" }, RespostaCorretaIndice = 2, Explicacao = "A bicicleta tem emissão zero de gases poluentes durante o trajeto! 🍀" },
                new QuizViewModel { Id = 1, Pergunta = "O que é 'Energia Limpa'?", Opcoes = new List<string> { "Energia de carvão", "Energia Solar/Eólica", "Energia de pilhas comuns" }, RespostaCorretaIndice = 1, Explicacao = "Fontes renováveis não emitem CO2 durante a geração." },
                new QuizViewModel { Id = 2, Pergunta = "Quanto tempo uma garrafa PET leva para se decompor?", Opcoes = new List<string> { "Até 100 anos", "Até 450 anos", "Cerca de 20 anos" }, RespostaCorretaIndice = 1, Explicacao = "Plásticos podem levar séculos. Reduzir o uso é fundamental! ♻️" },
                new QuizViewModel { Id = 3, Pergunta = "Qual dessas carnes tem a maior pegada de carbono?", Opcoes = new List<string> { "Frango", "Suína", "Bovina" }, RespostaCorretaIndice = 2, Explicacao = "A produção de carne bovina exige muito mais recursos e emite mais metano." },
                new QuizViewModel { Id = 4, Pergunta = "O que significa o termo 'Carbono Neutro'?", Opcoes = new List<string> { "Não respirar", "Equilibrar emissões com absorção", "Usar apenas pilhas" }, RespostaCorretaIndice = 1, Explicacao = "É quando compensamos o que emitimos através de ações como o plantio de árvores. 🌳" },
                new QuizViewModel { Id = 5, Pergunta = "Qual o maior benefício da compostagem doméstica?", Opcoes = new List<string> { "Gerar adubo e reduzir lixo", "Atrair insetos", "Aumentar o consumo" }, RespostaCorretaIndice = 0, Explicacao = "A compostagem transforma lixo orgânico em nutriente para a terra! 🍀" },
                new QuizViewModel { Id = 6, Pergunta = "Qual dessas lâmpadas é a mais eficiente?", Opcoes = new List<string> { "Incandescente", "Fluorescente", "LED" }, RespostaCorretaIndice = 2, Explicacao = "Lâmpadas LED consomem até 80% menos energia que as comuns." },
                new QuizViewModel { Id = 7, Pergunta = "O que é o 'Efeito Estufa'?", Opcoes = new List<string> { "Um tipo de horta", "Aquecimento global por gases", "Resfriamento da Terra" }, RespostaCorretaIndice = 1, Explicacao = "É o acúmulo de gases que retêm calor na atmosfera." },
                new QuizViewModel { Id = 8, Pergunta = "Qual país é líder mundial em energia eólica?", Opcoes = new List<string> { "Brasil", "China", "Estados Unidos" }, RespostaCorretaIndice = 1, Explicacao = "A China investe massivamente em infraestrutura de energias renováveis." },
                new QuizViewModel { Id = 9, Pergunta = "Qual a melhor forma de descartar eletrônicos?", Opcoes = new List<string> { "Lixo comum", "Pontos de coleta específica", "Queimar no quintal" }, RespostaCorretaIndice = 1, Explicacao = "Eletrônicos possuem metals pesados e devem ser reciclados em locais próprios. 🍀" },
                new QuizViewModel { Id = 10, Pergunta = "Quanto de água consome um banho de 15 minutos em média?", Opcoes = new List<string> { "Cerca de 30 litros", "Cerca de 135 litros", "Menos de 10 litros" }, RespostaCorretaIndice = 1, Explicacao = "Um banho longo consome muita água potável. Fechar o registro enquanto se ensaboa ajuda o planeta!" },
                new QuizViewModel { Id = 11, Pergunta = "O descarte inadequado de 1 litro de óleo de cozinha pode poluir até quantos litros de água?", Opcoes = new List<string> { "100 litros", "5.000 litros", "25.000 litros" }, RespostaCorretaIndice = 2, Explicacao = "O óleo cria uma película que impede a entrada de oxigênio na água, matando a vida aquática." },
                new QuizViewModel { Id = 12, Pergunta = "Qual destas cores representa a lixeira de reciclagem para Vidros?", Opcoes = new List<string> { "Verde", "Azul", "Amarelo" }, RespostaCorretaIndice = 0, Explicacao = "O verde é para o vidro, azul para papel e papelão, e amarelo para os metais. ♻️" },
                new QuizViewModel { Id = 13, Pergunta = "Você sabia que o mundo digital polui? O envio de 1 e-mail simples emite cerca de quanta de CO2?", Opcoes = new List<string> { "4 gramas", "1 quilo", "Zero emissão" }, RespostaCorretaIndice = 0, Explicacao = "Os servidores de internet gastam muita eletricidade! Excluir e-mails antigos limpa espaço e reduz o uso desses servidores." },
                new QuizViewModel { Id = 14, Pergunta = "Qual dessas ações mais ajuda a mitigar a pegada de carbono urbana?", Opcoes = new List<string> { "Plantar árvores nativas", "Comprar roupas sintéticas", "Deixar aparelhos em stand-by" }, RespostaCorretaIndice = 0, Explicacao = "As árvores absorvem o CO2 da atmosfera durante a fotossíntese, purificando o ar! 🌳" }
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

            // Remove todas as conquistas do usuário
            var conquistasUsuario = _context.Conquistas
                .Where(c => c.UsuarioId == usuarioId.Value)
                .ToList();

            _context.Conquistas.RemoveRange(conquistasUsuario);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        // 2. PÁGINA DO HISTÓRICO GERENCIAL (CRUD)
        public IActionResult Historico()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            // CORREÇÃO: Garante valor padrão caso a sessão retorne string vazia
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
            // CORREÇÃO: Proteção contra valor nulo na Session
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
                novoRegistro.EmissaoTotal = (double)(novoRegistro.Quantidade * tipoAtividade.FatorEmissao);
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
            // CORREÇÃO: Proteção contra valor nulo na Session
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
                registroEditado.EmissaoTotal = (double)(registroEditado.Quantidade * tipoAtividade.FatorEmissao);
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