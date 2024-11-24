using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.ViewsModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ProyectoHsj_Beta.Controllers
{
    public class RolsController : Controller
    {
        private readonly HoySeJuegaContext _context;

        public RolsController(HoySeJuegaContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Set<RolesPermisosGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_ROLES_PERMISOS")
                .ToListAsync();
            return View(roles);
        }
    }
}
