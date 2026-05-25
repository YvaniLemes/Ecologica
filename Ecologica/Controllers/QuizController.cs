using Microsoft.AspNetCore.Mvc;
using Ecologica.Data;
using Ecologica.Models;
using System.Collections.Generic;

namespace Ecologica.Controllers
{
    public class QuizController : Controller
    {
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
                },
                new QuizViewModel {
                    Pergunta = "Qual destas ações ajuda a reduzir o desperdício de água?",
                    Opcoes = new List<string> { "Lavar a calçada com mangueira", "Fechar a torneira ao escovar os dentes", "Deixar o chuveiro ligado enquanto ensaboa" },
                    RespostaCorretaIndice = 1,
                    Explicacao = "Pequenas atitudes diárias economizam milhares de litros de água por ano."
                },
                new QuizViewModel {
                    Pergunta = "O que significa o termo 'Consumo Consciente'?",
                    Opcoes = new List<string> { "Comprar apenas o necessário", "Comprar o mais barato", "Comprar itens de marca" },
                    RespostaCorretaIndice = 0,
                    Explicacao = "Consumo consciente envolve responsabilidade social e ambiental na escolha dos produtos."
                },
                new QuizViewModel {
                    Pergunta = "Quanto tempo, em média, uma garrafa plástica leva para se decompor na natureza?",
                    Opcoes = new List<string> { "10 anos", "100 anos", "Mais de 450 anos" },
                    RespostaCorretaIndice = 2,
                    Explicacao = "O plástico é um dos maiores poluentes devido ao seu longo tempo de decomposição."
                },
                new QuizViewModel {
                    Pergunta = "O que é o efeito estufa?",
                    Opcoes = new List<string> { "Um fenômeno natural que mantém a Terra aquecida", "Um aparelho para esquentar casas", "Uma poluição causada apenas por fábricas" },
                    RespostaCorretaIndice = 0,
                    Explicacao = "É um processo natural, mas está sendo intensificado pela ação humana, causando o aquecimento global."
                },
                new QuizViewModel {
                    Pergunta = "Qual material deve ser colocado na lixeira de coleta seletiva AZUL?",
                    Opcoes = new List<string> { "Vidro", "Papel/Papelão", "Metal" },
                    RespostaCorretaIndice = 1,
                    Explicacao = "A cor azul é o padrão internacional para a coleta de papéis."
                },
                new QuizViewModel {
                    Pergunta = "Qual destas fontes de energia é considerada não renovável?",
                    Opcoes = new List<string> { "Petróleo", "Energia Solar", "Energia das Ondas" },
                    RespostaCorretaIndice = 0,
                    Explicacao = "O petróleo é um combustível fóssil que um dia irá acabar."
                }
            };

            ViewBag.PerguntasQuiz = perguntas;

            return View();
        }
    }
}