using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IKart_ServerSide.Controllers.Admin
{
    [RoutePrefix("api/emicards")]
    public class EmiCardController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // GET: api/emicards
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var cards = (from cr in db.Card_Request
                         join u in db.Users on cr.UserId equals u.UserId
                         select new EmiCardDto
                         {
                             CardId = cr.Card_Id,
                             UserId = u.UserId,
                             UserName = u.FullName,
                             Email = u.Email,

                             ApprovalStatus = (cr.IsVerified == true) ? "Approved" : "Rejected",

                             Documents = db.EmiCard_Documents
                                           .Where(d => d.Card_Id == cr.Card_Id)
                                           .Select(d => new EmiCardDocumentDto
                                           {
                                               DocumentId = d.DocumentId,
                                               CardId = d.Card_Id ?? 0,
                                               DocumentType = d.DocumentType,
                                               FileName = d.FileName,
                                               FilePath = d.FilePath
                                           }).ToList()
                         }).ToList();

            return Ok(cards);
        }

        // GET: api/emicards/{id}
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var card = (from cr in db.Card_Request
                        join u in db.Users on cr.UserId equals u.UserId
                        where cr.Card_Id == id
                        select new EmiCardDto
                        {
                            CardId = cr.Card_Id,
                            UserId = u.UserId,
                            UserName = u.FullName,
                            Email = u.Email,

                            ApprovalStatus = (cr.IsVerified == true) ? "Approved" : "Rejected",

                            Documents = db.EmiCard_Documents
                                           .Where(d => d.Card_Id == cr.Card_Id)
                                           .Select(d => new EmiCardDocumentDto
                                           {
                                               DocumentId = d.DocumentId,
                                               CardId = d.Card_Id ?? 0,
                                               DocumentType = d.DocumentType,
                                               FileName = d.FileName,
                                               FilePath = d.FilePath
                                           }).ToList()
                        }).FirstOrDefault();

            if (card == null)
                return NotFound();

            return Ok(card);
        }

        // PUT: api/emicards/updatestatus/5
        [HttpPut, Route("updatestatus/{id:int}")]
        public IHttpActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var card = db.Card_Request.FirstOrDefault(c => c.Card_Id == id);
            if (card == null) return NotFound();

            if (status == "Approved")
                card.IsVerified = true;
            else if (status == "Rejected")
                card.IsVerified = false;

            db.SaveChanges();
            return Ok("Approval status updated");
        }
    }
}
