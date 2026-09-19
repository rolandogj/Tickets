using GlobalTech.Data;
using GlobalTech.DTOs;
using GlobalTech.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalTech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class BitacoraController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BitacoraController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/bitacora?pageIndex=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<BitacoraResponseDto>>> GetBitacora(
            [FromQuery] int? idTicket,
            [FromQuery] int? idUsuario,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            // validar paginación
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = from b in _context.Bitacora
                        join t in _context.Tickets on b.IdTicket equals t.IdTicket
                        join u in _context.Usuarios on b.IdUsuario equals u.IdUsuario
                        select new BitacoraResponseDto
                        {
                            IdBitacora = b.IdBitacora,
                            IdTicket = b.IdTicket,
                            IdUsuario = b.IdUsuario,
                            NombreUsuario = $"{u.Nombre} {u.Apellido}",
                            Comentario = b.Comentario,
                            EstadoAnterior = b.EstadoAnterior ?? string.Empty,
                            EstadoNuevo = b.EstadoNuevo,
                            Fecha = b.Fecha
                        };


            // 1. Filtro opcional por Ticket específico
            if (idTicket.HasValue)
            {
                query = query.Where(b => b.IdTicket == idTicket.Value);
            }

            // 2. Filtro opcional por Usuario que realizó la acción
            if (idUsuario.HasValue)
            {
                query = query.Where(b => _context.Bitacora.Any(bit => bit.IdBitacora == b.IdBitacora && bit.IdUsuario == idUsuario.Value));
            }

            // Calcular el total de registros de la bitácora según los filtros aplicados
            var totalRegistros = await query.CountAsync();

            // Aplicar orden descendente por fecha (más recientes primero) y paginación
            var items = await query
                .OrderByDescending(b => b.Fecha)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResultDto<BitacoraResponseDto>
            {
                Items = items,
                TotalRegistros = totalRegistros,
                PaginaActual = pageIndex,
                TamanoPagina = pageSize
            };

            return Ok(result);
        }

        // GET: api/bitacora/ticket/10
        [HttpGet("ticket/{idTicket}")]
        public async Task<ActionResult<IEnumerable<BitacoraResponseDto>>> GetBitacoraByTicket(int idTicket)
        {
            var historial = await _context.Bitacora
                .Where(b => b.IdTicket == idTicket)
                .Join(
                    _context.Usuarios,
                    b => b.IdUsuario,
                    u => u.IdUsuario,
                    (b, u) => new BitacoraResponseDto
                    {
                        IdBitacora = b.IdBitacora,
                        IdTicket = b.IdTicket,
                        IdUsuario = b.IdUsuario,
                        NombreUsuario = $"{u.Nombre} {u.Apellido}".Trim(), // Nombre completo
                        Comentario = b.Comentario,
                        EstadoAnterior = b.EstadoAnterior ?? string.Empty,
                        EstadoNuevo = b.EstadoNuevo,
                        Fecha = b.Fecha
                    }
                )
                .OrderByDescending(b => b.Fecha)
                .ToListAsync();

            return Ok(historial); // Retorna [] si no hay registros, evitando el 404
        }
    }
}
