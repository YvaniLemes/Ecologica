using Microsoft.AspNetCore.Mvc; 
using Ecologica.Models.Data; // Para encontrar a pasta Data
using Ecologica.Models;      // Para encontrar os modelos     
using System.Collections.Generic;

namespace Ecologica.Controllers
{
    public class QuizController : Controller
    {
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

            return View(perguntas);
        }
    }
}