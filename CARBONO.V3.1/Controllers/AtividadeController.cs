using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CARBONO.V3._1.Models;
using CARBONO.V3._1.Data;
using Microsoft.AspNetCore.Http; // Necessário para usar Session

namespace CARBONO.V3._1.Controllers
{
    public class AtividadeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AtividadeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listagem do histórico filtrado por usuário
        public IActionResult Index()
        {
            // Recupera os dados da sessão gravados no momento do Login
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var nomeLogado = HttpContext.Session.GetString("UsuarioNome");

            // Redireciona para o login caso a sessão tenha expirado ou não exista
            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            // Exibe o nome real do usuário logado na View
            ViewBag.NomeUsuario = nomeLogado; 

            // Filtra os registros para que apareçam apenas os do usuário atual
            var registros = _context.RegistrosCarbono
                .Where(r => r.IdUsuario == usuarioId) 
                .Include(r => r.AtividadeRelacionada)
                .ToList();

            return View(registros);
        }

        public IActionResult Create()
        {
            ViewBag.ListaAtividades = _context.Atividades.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(RegistroCarbono novoRegistro)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var tipoAtividade = _context.Atividades.Find(novoRegistro.IdAtividade);

            if (tipoAtividade != null && usuarioId != null)
            {
                // Vincula o registro ao ID dinâmico da sessão, não mais ao fixo "1"
                novoRegistro.IdUsuario = usuarioId.Value; 
                novoRegistro.EmissaoTotal = novoRegistro.Quantidade * tipoAtividade.FatorEmissao;
                novoRegistro.DataRegistro = DateTime.Now;

                _context.RegistrosCarbono.Add(novoRegistro);
                _context.SaveChanges();
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
        public IActionResult Edit(RegistroCarbono registroEditado)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var tipoAtividade = _context.Atividades.Find(registroEditado.IdAtividade);

            if (tipoAtividade != null && usuarioId != null)
            {
                // Garante que a edição também respeite o dono do registro
                registroEditado.IdUsuario = usuarioId.Value; 
                registroEditado.EmissaoTotal = registroEditado.Quantidade * tipoAtividade.FatorEmissao;
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

            // Só permite deletar se o registro pertencer ao usuário logado
            if (registro != null && registro.IdUsuario == usuarioId)
            {
                _context.RegistrosCarbono.Remove(registro);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}