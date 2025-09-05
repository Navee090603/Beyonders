using IKart_ServerSide.Models;
using IKart_Shared.DTOs;
using System.Linq;
using System.Web.Http;

namespace IKart_ServerSide.Controllers
{
    [RoutePrefix("api/emicards")]
    public class EmiCardController : ApiController
    {
        // ✅ Only ONE db instance (fix for CS0229 ambiguity)
        private readonly IKartEntities db = new IKartEntities();

        // GET: api/emicards
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var cards = (from ec in db.EMI_Card
                         join u in db.Users on ec.UserId equals u.UserId
                         join jf in db.Joining_Fee on ec.EmiCardId equals jf.Card_Id into feeJoin
                         from jf in feeJoin.DefaultIfEmpty()
                         select new EmiCardDto
                         {
                             CardId = ec.EmiCardId,
                             UserId = u.UserId,
                             UserName = u.FullName,
                             Email = u.Email,

                             FeePaid = (jf.Amount ?? 0),
                             FeeStatus = jf != null ? jf.Status : "Not Paid",

                             ApprovalStatus = (ec.IsActive ?? false) ? "Approved" : "Pending",

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
                         }).ToList();

            return Ok(cards);
        }

        // GET: api/emicards/{id}
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var card = (from ec in db.EMI_Card
                        join u in db.Users on ec.UserId equals u.UserId
                        join jf in db.Joining_Fee on ec.EmiCardId equals jf.Card_Id into feeJoin
                        from jf in feeJoin.DefaultIfEmpty()
                        where ec.EmiCardId == id
                        select new EmiCardDto
                        {
                            CardId = ec.EmiCardId,
                            UserId = u.UserId,
                            UserName = u.FullName,
                            Email = u.Email,

                            FeePaid = (jf.Amount ?? 0),
                            FeeStatus = jf != null ? jf.Status : "Not Paid",

                            ApprovalStatus = (ec.IsActive ?? false) ? "Approved" : "Pending",

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

        // PUT: api/emicards/updatestatus/5
        [HttpPut, Route("updatestatus/{id:int}")]
        public IHttpActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var card = db.EMI_Card.FirstOrDefault(c => c.EmiCardId == id);
            if (card == null) return NotFound();

            if (status == "Approved")
                card.IsActive = true;
            else if (status == "Rejected")
                card.IsActive = false;

            db.SaveChanges();
            return Ok("Approval status updated");
        }
    }
}
