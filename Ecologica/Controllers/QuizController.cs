using Microsoft.AspNetCore.Mvc;
using Ecologica.Models.Data;
using Ecologica.Models;
using System.Collections.Generic;

namespace Ecologica.Controllers
{
    public class QuizController : Controller
    {
        // Se no futuro você quiser salvar a pontuação, o Context já está aqui
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
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

            // Usando a ViewBag exatamente como você tem na View
            ViewBag.PerguntasQuiz = perguntas;

            return View();
        }
    }
}