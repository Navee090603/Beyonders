using IKart_ServerSide.Models;
using IKart_Shared.DTOs;
using System.Linq;
using System.Web.Http;

namespace IKart_ServerSide.Controllers
{
    [RoutePrefix("api/emicards")]
    public class EmiCardController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        //// GET: api/emicards
        //[HttpGet, Route("")]
        //public IHttpActionResult GetAll()
        //{
        //    var cards = (from cr in db.Card_Request
        //                 join u in db.Users on cr.UserId equals u.UserId
        //                 join jf in db.Joining_Fee on cr.Card_Id equals jf.Card_Id
        //                 select new EmiCardDto
        //                 {
        //                     CardId = cr.Card_Id,                            // int (not nullable)
        //                     UserId = u.UserId,                              // int (not nullable)
        //                     UserName = u.FullName,
        //                     Email = u.Email,
        //                     FeePaid = jf.Amount ?? 0,                       // decimal? → decimal
        //                     Status = jf.Status,
        //                     Documents = db.EmiCard_Documents
        //                                    .Where(d => d.Card_Id == cr.Card_Id)
        //                                    .Select(d => new EmiCardDocumentDto
        //                                    {
        //                                        DocumentId = d.DocumentId,  // int (not nullable)
        //                                        CardId = d.Card_Id ?? 0,    // int? → int
        //                                        DocumentType = d.DocumentType,
        //                                        FileName = d.FileName,
        //                                        FilePath = d.FilePath
        //                                    }).ToList()
        //                 }).ToList();

        //    return Ok(cards);
        //}



        //// GET: api/emicards/{id}
        //[HttpGet, Route("{id:int}")]
        //public IHttpActionResult GetById(int id)
        //{
        //    var card = (from cr in db.Card_Request
        //                join u in db.Users on cr.UserId equals u.UserId
        //                join jf in db.Joining_Fee on cr.Card_Id equals jf.Card_Id
        //                where cr.Card_Id == id
        //                select new EmiCardDto
        //                {
        //                    CardId = cr.Card_Id,                            // int
        //                    UserId = u.UserId,                              // int
        //                    UserName = u.FullName,
        //                    Email = u.Email,
        //                    FeePaid = jf.Amount ?? 0,                       // decimal? → decimal
        //                    Status = jf.Status,
        //                    Documents = db.EmiCard_Documents
        //                                   .Where(d => d.Card_Id == cr.Card_Id)
        //                                   .Select(d => new EmiCardDocumentDto
        //                                   {
        //                                       DocumentId = d.DocumentId,  // int
        //                                       CardId = d.Card_Id ?? 0,    // int? → int
        //                                       DocumentType = d.DocumentType,
        //                                       FileName = d.FileName,
        //                                       FilePath = d.FilePath
        //                                   }).ToList()
        //                }).FirstOrDefault();

        //    if (card == null)
        //        return NotFound();

        //    return Ok(card);
        //}

        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var cards = (from ec in db.EMI_Card
                         join u in db.Users on ec.UserId equals u.UserId
                         select new EmiCardDto
                         {
                             CardId = ec.EmiCardId,   // ✅ use correct PK column
                             UserId = u.UserId,
                             UserName = u.FullName,
                             Email = u.Email,
                             FeePaid = db.Payments
                                         .Where(p => p.EmiCardId == ec.EmiCardId)
                                         .Sum(p => (decimal?)p.ProcessingFee) ?? 0,
                             Status = (ec.IsActive ?? false) ? "Active" : "Inactive", // ✅ handle bool?
                             Documents = db.EmiCard_Documents
                                            .Where(d => d.Card_Id == ec.EmiCardId)   // ✅ match FK
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

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var card = (from ec in db.EMI_Card
                        join u in db.Users on ec.UserId equals u.UserId
                        where ec.EmiCardId == id  // ✅ use correct PK
                        select new EmiCardDto
                        {
                            CardId = ec.EmiCardId,
                            UserId = u.UserId,
                            UserName = u.FullName,
                            Email = u.Email,
                            FeePaid = db.Payments
                                        .Where(p => p.EmiCardId == ec.EmiCardId)
                                        .Sum(p => (decimal?)p.ProcessingFee) ?? 0,
                            Status = (ec.IsActive ?? false) ? "Active" : "Inactive",
                            Documents = db.EmiCard_Documents
                                           .Where(d => d.Card_Id == ec.EmiCardId)
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


        [HttpPut]
        [Route("{id}/approve")]
        public IHttpActionResult ApproveCard(int id)
        {
            var fee = db.Joining_Fee.FirstOrDefault(j => j.Card_Id == id);
            if (fee == null) return NotFound();

            fee.Status = "Approved";
            db.SaveChanges();
            return Ok(fee);
        }

        [HttpPut]
        [Route("{id}/reject")]
        public IHttpActionResult RejectCard(int id)
        {
            var fee = db.Joining_Fee.FirstOrDefault(j => j.Card_Id == id);
            if (fee == null) return NotFound();

            fee.Status = "Rejected";
            db.SaveChanges();
            return Ok(fee);
        }




        // PUT: api/emicards/updatestatus/5
        [HttpPut, Route("updatestatus/{id:int}")]
        public IHttpActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var fee = db.Joining_Fee.FirstOrDefault(j => j.Card_Id == id);
            if (fee == null) return NotFound();

            fee.Status = status; // "Approved" or "Rejected"
            db.SaveChanges();

            return Ok("Status updated");
        }
    }
}
