using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CARBONO.V3._1.Models;
using CARBONO.V3._1.Data;

namespace CARBONO.V3._1.Controllers
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
            var usuarioEncontrado = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Senha == senha);

            if (usuarioEncontrado != null)
            {
                HttpContext.Session.SetString("NomeUsuario", usuarioEncontrado.Nome);
                HttpContext.Session.SetInt32("UsuarioId", usuarioEncontrado.Id);
                return RedirectToAction("Index", "Home");
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
                // Verifica se o e-mail já existe no banco
                var existe = await _context.Usuarios.AnyAsync(u => u.Email == novoUsuario.Email);
                if (existe)
                {
                    ViewBag.Erro = "Este e-mail já está cadastrado!";
                    return View(novoUsuario);
                }

                // Resolve o erro 'NOT NULL constraint failed' forçando o zero
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