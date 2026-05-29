using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecologica.Data;
using Ecologica.Models;      // Para encontrar os modelos 
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecologica.Controllers


{
    public class RegistrosCarbonoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrosCarbonoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            // Filtre pelo usuário para não mostrar dados de outros usuários
            var registros = await _context.RegistrosCarbono
                .Where(r => r.IdUsuario == usuarioId)
                .Include(r => r.AtividadeRelacionada)
                .ToListAsync();

            return View(registros);
        }
    }
}