using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GlobalTech.Data;
using GlobalTech.DTOs;
using GlobalTech.Models;
using Microsoft.AspNetCore.Authorization;

namespace GlobalTech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Obtener todos los tickets
        // GET: api/tickets (soporta filtros opcionales y nombres de usuario/técnico proyectados)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetTickets(
            [FromQuery] string? estado,
            [FromQuery] int? idTecnico,
            [FromQuery] int? idUsuario,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
                        
        {
            // Validar parámetros de paginación
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; 

            var query = from t in _context.Tickets
                        join u in _context.Usuarios on t.IdUsuario equals u.IdUsuario
                        join d in _context.Departamentos on u.IdDepartamento equals d.IdDepartamento
                        join tec in _context.Usuarios on t.IdTecnico equals tec.IdUsuario into tecGroup
                        from tecAsignado in tecGroup.DefaultIfEmpty() // Left Join para técnico asignado
                        select new TicketResponseDto
                        {
                            IdTicket = t.IdTicket,
                            UsuarioSolicitante = $"{u.Nombre} {u.Apellido}",
                            TecnicoAsignado = tecAsignado != null ? $"{tecAsignado.Nombre} {tecAsignado.Apellido}" : "Sin Asignar",
                            Departamento = d.NombreDepartamento,
                            Descripcion = t.Descripcion,
                            Prioridad = t.Prioridad,
                            Estado = t.Estado,
                            FechaCreacion = t.FechaCreacion
                        };

            // 1. Filtrar por estado si se indica
            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(t => t.Estado.ToLower() == estado.ToLower());
            }

            // 2. Filtrar por técnico asignado si se indica
            if (idTecnico.HasValue)
            {
                query = query.Where(t => _context.Tickets.Any(ticket => ticket.IdTicket == t.IdTicket && ticket.IdTecnico == idTecnico.Value));
            }

            // 3. Filtrar por usuario creador si se indica
            if (idUsuario.HasValue)
            {
                query = query.Where(t => _context.Tickets.Any(ticket => ticket.IdTicket == t.IdTicket && ticket.IdUsuario == idUsuario.Value));
            }
            // Calcular el total de registros filtrados antes de aplicar Skip/Take
            var totalRegistros = await query.CountAsync();

            // Aplicar ordenamiento y paginación
            var items = await query
                .OrderByDescending(t => t.FechaCreacion)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResultDto<TicketResponseDto>
            {
                Items = items,
                TotalRegistros = totalRegistros,
                PaginaActual = pageIndex,
                TamanoPagina = pageSize
            };

            return Ok(result);
        }

        // 2. Crear un nuevo ticket
        [HttpPost]
        public async Task<ActionResult<TicketResponseDto>> CreateTicket([FromBody] TicketCreateDto ticketDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticket = new Ticket
            {
                IdUsuario = ticketDto.IdUsuario,
                Descripcion = ticketDto.Descripcion,
                Prioridad = ticketDto.Prioridad,
                Estado = "Abierto", // Estado inicial
                FechaCreacion = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTickets), new { id = ticket.IdTicket }, ticketDto);
        }

        // 3. Reporte Especial: "Alerta Roja" (Departamentos con mas de 5 tickets criticos)
        [HttpGet("reporte/alerta-roja")]
        public async Task<ActionResult<IEnumerable<RedAlertReportDto>>> GetAlertReport()
        {
            var reporte = await _context.Tickets
                .Where(t => t.Prioridad == "Crítica")
                .GroupBy(t => new { t.Usuario!.IdDepartamento, t.Usuario.Departamento!.NombreDepartamento })
                .Where(g => g.Count() > 5)
                .Select(g => new RedAlertReportDto
                {
                    IdDepartamento = g.Key.IdDepartamento,
                    NombreDepartamento = g.Key.NombreDepartamento,
                    TotalTicketsCriticos = g.Count()
                })
                .ToListAsync();

            return Ok(reporte);
        }

        // 4. Actualizar el estado de un ticket
        [HttpPut("{id}/estado")]
        public async Task<ActionResult> UpdateTicketStatus(int id, [FromBody] TicketUpdateStatusDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. buscar ticket existente
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound(new { mensaje = $"El ticket con ID {id} no existe" });
            }

            // Bloquear tickets ya cerrados
            if (ticket.Estado == "Cerrado")
            {
                return BadRequest(new { mensaje = $"El ticket con ID {id} ya está cerrado y no puede ser modificado." });
            }

            string estadoAnterior = ticket.Estado;

            // si el estado no cambia, se ignora la transacción
            if (estadoAnterior.Equals(updateDto.NuevoEstado, StringComparison.OrdinalIgnoreCase))
            { 
                return BadRequest(new { mensaje = $"El ticket ya se encuentra en estado '{ticket.Estado}'." });
            }

            // usar transnacción explicita para garantizar consistencia atómica
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Actualizar el estado del ticket
                ticket.Estado = updateDto.NuevoEstado;
                _context.Tickets.Update(ticket);

                // 3. Registrar el cambio en la bitacora
                var bitacora = new Bitacora
                {
                    IdTicket = ticket.IdTicket,
                    IdUsuario = updateDto.IdUsuario,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = updateDto.NuevoEstado,
                    Fecha = DateTime.UtcNow,
                };
                
                _context.Bitacora.Add(bitacora);

                // 4. Guardar cambios y confirmar transacción
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "Estado del ticket actualizado y registrado en bitácora correctamente.",
                    idTicket = ticket.IdTicket,
                    estadoAnterior = estadoAnterior,
                    nuevoEstado = ticket.Estado
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new
                {
                    mensaje = "Error al actualizar el estado del ticket.",
                    detalle = errorDetalle
                });
            }
        }

        // 5. Asignar Tecnico a un ticket
        [HttpPut("{id}/asignar")]
        public async Task<IActionResult> AssignTecnico(int id, [FromBody] TicketAssignDto assignDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Validar existencia del ticket
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound(new { mensaje = $"El ticket con ID {id} no existe." });
            }

            // 2. Prevenir asignación de tickets cerrados
            if (ticket.Estado == "Cerrado")
            {
                return BadRequest(new { mensaje = $"El ticket con ID {id} ya está cerrado y no puede ser modificado." });
            }

            // 3. Validar que el técnico exista en la BD
            var tecnico = await _context.Usuarios.FindAsync(assignDto.IdTecnico);
            if (tecnico == null)
            {
                return NotFound(new { mensaje = $"El usuario/técnico con ID {assignDto.IdTecnico} no existe." });
            }

            // 4. Evaluar si el técnico asignado es el mismo
            if (ticket.IdTecnico == assignDto.IdTecnico)
            {
                return BadRequest(new { mensaje = $"El ticket ya está asignado al técnico con ID {assignDto.IdTecnico}." });
            }

            int? tecnicoAnterior = ticket.IdTecnico;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 4. Actualizar asignación en el ticket
                ticket.IdTecnico = assignDto.IdTecnico;
                _context.Tickets.Update(ticket);

                // 5. Registrar en la Bitácora
                string mensajeAuditoria = tecnicoAnterior.HasValue
                    ? $"Reasignación de técnico: ID {tecnicoAnterior.Value} -> ID {assignDto.IdTecnico}"
                    : $"Asignación inicial de técnico: ID {assignDto.IdTecnico}";

                var bitacora = new Bitacora
                {
                    IdTicket = ticket.IdTicket,
                    IdUsuario = assignDto.IdUsuarioAsignador,
                    EstadoAnterior = ticket.Estado,
                    EstadoNuevo = ticket.Estado,
                    Comentario = mensajeAuditoria,
                    Fecha = DateTime.UtcNow
                };

                _context.Bitacora.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "Técnico asignado y cambio registrado en bitácora exitosamente.",
                    idTicket = ticket.IdTicket,
                    idTecnicoAnterior = tecnicoAnterior,
                    idTecnicoAsignado = assignDto.IdTecnico
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { mensaje = "Error al asignar el técnico al ticket.", detalle = errorDetalle });
            }
        }

        // 6. Resolver un ticket (cambiar estado a "Resuelto")
        // PUT: api/tickets/10/resolver
        [HttpPut("{id}/resolver")]
        public async Task<IActionResult> ResolveTicket(int id, [FromBody] TicketResolveDto resolveDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound(new { mensaje = $"El ticket con ID {id} no existe." });
            }

            // No se puede resolver/cerrar un ticket que ya está cerrado
            if (ticket.Estado == "Cerrado")
            {
                return BadRequest(new { mensaje = "El ticket ya se encuentra cerrado y no se puede modificar." });
            }

            string estadoAnterior = ticket.Estado;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Actualizar estado del ticket
                ticket.Estado = resolveDto.EstadoFinal;
                _context.Tickets.Update(ticket);

                // 2. Registrar en la bitácora con la solución como comentario
                var bitacora = new Bitacora
                {
                    IdTicket = ticket.IdTicket,
                    IdUsuario = resolveDto.IdUsuario,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = resolveDto.EstadoFinal,
                    Comentario = $"[SOLUCIÓN]: {resolveDto.Solucion}",
                    Fecha = DateTime.UtcNow
                };

                _context.Bitacora.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = $"El ticket #{ticket.IdTicket} ha sido marcado como '{resolveDto.EstadoFinal}' exitosamente.",
                    idTicket = ticket.IdTicket,
                    EstadoAnterior = estadoAnterior,
                    nuevoEstado = resolveDto.EstadoFinal,
                    solucion = resolveDto.Solucion
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { mensaje = "Error al resolver/cerrar el ticket.", detalle = errorDetalle });
            }
        }

        // 7. Obteniendo tickets por usuario
        // GET: api/tickets/10
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketResponseDto>> GetTicketById(int id)
        {
            var ticket = await (from t in _context.Tickets
                                join u in _context.Usuarios on t.IdUsuario equals u.IdUsuario
                                join d in _context.Departamentos on u.IdDepartamento equals d.IdDepartamento
                                join tec in _context.Usuarios on t.IdTecnico equals tec.IdUsuario into tecGroup
                                from tecAsignado in tecGroup.DefaultIfEmpty()
                                where t.IdTicket == id
                                select new TicketResponseDto
                                {
                                    IdTicket = t.IdTicket,
                                    UsuarioSolicitante = $"{u.Nombre} {u.Apellido}",
                                    TecnicoAsignado = tecAsignado != null ? $"{tecAsignado.Nombre} {tecAsignado.Apellido}" : "Sin Asignar",
                                    Departamento = d.NombreDepartamento,
                                    Descripcion = t.Descripcion,
                                    Prioridad = t.Prioridad,
                                    Estado = t.Estado,
                                    FechaCreacion = t.FechaCreacion
                                }).FirstOrDefaultAsync();

            if (ticket == null)
            {
                return NotFound(new { mensaje = $"El ticket con ID {id} no existe." });
            }

            return Ok(ticket);
        }

        //8. Obtener métricas de tickets
        // GET: api/tickets/metricas
        [HttpGet("metricas")]
        public async Task<ActionResult<TicketMetricasDto>> GetMetricasTickets()
        {
            // Agrupar por el campo Estado y contar los registros
            var conteoPorEstado = await _context.Tickets
                .GroupBy(t => t.Estado.ToLower())
                .Select(g => new
                {
                    Estado = g.Key,
                    Total = g.Count()
                })
                .ToListAsync();

            var metricas = new TicketMetricasDto
            {
                TotalTickets = conteoPorEstado.Sum(x => x.Total),
                Abiertos = conteoPorEstado.FirstOrDefault(x => x.Estado == "abierto")?.Total ?? 0,
                EnProceso = conteoPorEstado.FirstOrDefault(x => x.Estado == "en proceso")?.Total ?? 0,
                Resueltos = conteoPorEstado.FirstOrDefault(x => x.Estado == "resuelto")?.Total ?? 0,
                Cerrados = conteoPorEstado.FirstOrDefault(x => x.Estado == "cerrado")?.Total ?? 0
            };

            return Ok(metricas);
        }

        //9. Solo usuarios con Claim de Rol "Admin" pueden eliminar tickets
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteTicket(int id)
        {
            return NoContent();
        }

        //10. Permitir acceso a Admins o Técnicos
        [HttpPut("api/tickets/estado")]
        [Authorize(Roles = "Admin,Tecnico")]
        public IActionResult UpdateStatus(int id, [FromBody] string nuevoEstado)
        {
            return Ok();
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetProfile()
        {
            // Obtener el ID o Email desde las claims del token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            return Ok(new { UserId = userId, Email = userEmail });
        }
    }
}
