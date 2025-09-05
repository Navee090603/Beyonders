using System;
using System.Linq;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;

namespace IKart_ServerSide.Controllers.Api
{
    [RoutePrefix("api/supporttickets")]
    public class SupportTicketsController : ApiController
    {
        IKartEntities db = new IKartEntities();

        // ✅ Get all tickets
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetTickets()
        {
            var tickets = db.Support_Tickets.Select(t => new SupportTicketDto
            {
                TicketId = t.TicketId,
                UserId = (int)t.UserId,
                Subject = t.Subject,
                Description = t.Description,
                Status = t.Status,
                CreatedDate = (DateTime)t.CreatedDate,
                ClosedDate = t.ClosedDate
            }).ToList();

            return Ok(tickets);
        }

        // ✅ Get by Id
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetTicket(int id)
        {
            var ticket = db.Support_Tickets.Where(t => t.TicketId == id).Select(t => new SupportTicketDto
            {
                TicketId = t.TicketId,
                UserId = (int)t.UserId,
                Subject = t.Subject,
                Description = t.Description,
                Status = t.Status,
                CreatedDate = (DateTime)t.CreatedDate,
                ClosedDate = t.ClosedDate
            }).FirstOrDefault();

            if (ticket == null) return NotFound();
            return Ok(ticket);
        }

        // ✅ Create new ticket
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateTicket(SupportTicketDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ticket = new Support_Tickets
            {
                UserId = dto.UserId,
                Subject = dto.Subject,
                Description = dto.Description,
                Status = "Open",
                CreatedDate = DateTime.Now
            };

            db.Support_Tickets.Add(ticket);
            db.SaveChanges();

            dto.TicketId = ticket.TicketId;
            dto.CreatedDate = (DateTime)ticket.CreatedDate;
            return Ok(dto);
        }

        // ✅ Update status
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdateTicket(int id, SupportTicketDto dto)
        {
            var ticket = db.Support_Tickets.Find(id);
            if (ticket == null) return NotFound();

            ticket.Status = dto.Status;
            ticket.ClosedDate = dto.ClosedDate;
            db.SaveChanges();

            return Ok(dto);
        }

        // ✅ Delete
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteTicket(int id)
        {
            var ticket = db.Support_Tickets.Find(id);
            if (ticket == null) return NotFound();

            db.Support_Tickets.Remove(ticket);
            db.SaveChanges();
            return Ok();
        }
    }
}
