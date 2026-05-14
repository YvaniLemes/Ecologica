using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ecologica.Data;
using Ecologica.Models;      
using System.Collections.Generic;

namespace Ecologica.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // 1. Definimos o nome do usuário para o Dashboard
        ViewBag.NomeUsuario = "Yvani";

        // 2. Criamos as perguntas do Quiz
        var listaDePerguntas = new List<QuizViewModel>
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

        // 3. Enviamos a lista explicitamente para a ViewBag que a View está procurando
        ViewBag.PerguntasQuiz = listaDePerguntas;

        // 4. Inicializamos dados vazios para evitar erros de referência nula no Gráfico e Conquistas
        ViewBag.LabelsGrafico = new List<string>();
        ViewBag.ValoresGrafico = new List<double>();
        ViewBag.Conquistas = new List<Conquista>();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}