using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Ecologica.Models.Data; 
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
            // --- ACESSO DE TESTE / EMERGÊNCIA (Para a Equipe e Apresentação) ---
            // Permite logar sem depender do banco de dados local de cada um
            if (senha == "admin123" && (email == "marcelo@ecologica.com" || email == "elton@ecologica.com" || email == "yvani@ecologica.com"))
            {
                string nomeLimpo = email.Split('@')[0];
                string nomeFormatado = char.ToUpper(nomeLimpo[0]) + nomeLimpo.Substring(1);
                
                HttpContext.Session.SetString("UsuarioNome", nomeFormatado);
                HttpContext.Session.SetInt32("UsuarioId", 999); // ID fictício para teste
                
                return RedirectToAction("Index", "Atividade");
            }

            // --- LOGIN VIA BANCO DE DATA (SQLITE) ---
            var usuarioEncontrado = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Senha == senha);

            if (usuarioEncontrado != null)
            {
                // UsuarioNome deve bater com o que o AtividadeController busca
                HttpContext.Session.SetString("UsuarioNome", usuarioEncontrado.Nome ?? "");
                HttpContext.Session.SetInt32("UsuarioId", usuarioEncontrado.Id);
                
                return RedirectToAction("Index", "Atividade");
            }

            ViewBag.Erro = "E-mail ou senha incorretos!";
            return View();
        }

        public IActionResult Cadastro() => View();

        [HttpPost]
        public async Task<IActionResult> Cadastro(Usuario novoUsuario)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Usuarios.AnyAsync(u => u.Email == novoUsuario.Email);
                if (existe)
                {
                    ViewBag.Erro = "Este e-mail já está cadastrado!";
                    return View(novoUsuario);
                }

                novoUsuario.Pontos = 0;

                _context.Usuarios.Add(novoUsuario);
                await _context.SaveChangesAsync(); 

                return RedirectToAction("Login");
            }
            return View(novoUsuario);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}