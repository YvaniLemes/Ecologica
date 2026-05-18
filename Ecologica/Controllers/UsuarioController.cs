using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Ecologica.Data;
using Ecologica.Models;

namespace Ecologica.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string senha)
        {
            // Acesso de teste para equipe
            if (senha == "admin123" && (email == "marcelo@ecologica.com" || email == "elton@ecologica.com" || email == "yvani@ecologica.com"))
            {
                string nomeLimpo = email.Split('@')[0];
                string nomeFormatado = char.ToUpper(nomeLimpo[0]) + nomeLimpo.Substring(1);

                // AJUSTADO: "UsuarioNome" para bater com o que o AtividadeController espera
                HttpContext.Session.SetString("UsuarioNome", nomeFormatado);
                HttpContext.Session.SetInt32("UsuarioId", 999);
                return RedirectToAction("Index", "Atividade");
            }

            // Busca o usuário de forma assíncrona
            var usuarioEncontrado = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Senha == senha);

            // GARANTIA: Verificação exata da variável correta ("usuarioEncontrado")
            if (usuarioEncontrado != null)
            {
                // AJUSTADO: "UsuarioNome" garantido com fallback seguro para evitar avisos CS8602
                HttpContext.Session.SetString("UsuarioNome", usuarioEncontrado.Nome ?? "Usuário");
                HttpContext.Session.SetInt32("UsuarioId", usuarioEncontrado.Id);
                return RedirectToAction("Index", "Atividade");
            }

            // AVISO DE ERRO NO LOGIN
            ViewBag.Erro = "E-mail ou senha incorretos! Verifique seus dados. 🍀";
            return View();
        }

        public IActionResult Cadastro() => View();

        [HttpPost]
        public async Task<IActionResult> Cadastro(Usuario novoUsuario)
        {
            // Validação manual para garantir que o usuário preencheu o básico
            if (string.IsNullOrEmpty(novoUsuario.Nome) || string.IsNullOrEmpty(novoUsuario.Email) || string.IsNullOrEmpty(novoUsuario.Senha))
            {
                ViewBag.Erro = "Por favor, preencha todos os campos obrigatórios!";
                return View(novoUsuario);
            }

            var existe = await _context.Usuarios.AnyAsync(u => u.Email == novoUsuario.Email);
            if (existe)
            {
                // AVISO DE ERRO NO CADASTRO
                ViewBag.Erro = "Este e-mail já está sendo usado por outro protetor do planeta! 🌍";
                return View(novoUsuario);
            }

            try
            {
                novoUsuario.Pontos = 0;
                _context.Usuarios.Add(novoUsuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            catch (System.Exception)
            {
                ViewBag.Erro = "Ops! Houve um erro técnico ao salvar. Tente novamente em instantes.";
                return View(novoUsuario);
            }
        }

        //MÉTODO DE LOGOUT QUE FAZ O "VOLTAR" FUNCIONAR COM SEGURANÇA
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}