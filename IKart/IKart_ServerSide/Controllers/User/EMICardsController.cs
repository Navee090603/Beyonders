using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs.EMI_Card;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/emicards")]
    public class EMICardsController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // 1️⃣ Get all EMI cards for a user
        [HttpGet]
        [Route("user/{userId}")]
        public IHttpActionResult GetUserCards(int userId)
        {
            if (!db.Users.Any(u => u.UserId == userId))
                return BadRequest("Invalid UserId");

            var cards = db.EMI_Card
                .Where(c => c.UserId == userId)
                .Select(c => new EmiCardDto
                {
                    EmiCardId = c.EmiCardId,
                    UserId = (int)c.UserId,
                    CardType = c.CardType,
                    CardNumber = c.CardNumber,
                    TotalLimit = (decimal)c.TotalLimit,
                    Balance = (decimal)c.Balance,
                    IsActive = (bool)c.IsActive,
                    IssueDate = c.IssueDate,
                    ExpireDate = c.ExpireDate
                }).ToList();

            return Ok(cards);
        }

        // 2️⃣ Request new card
        [HttpPost]
        [Route("request")]
        public IHttpActionResult RequestCard(CardRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            if (!db.Users.Any(u => u.UserId == dto.UserId))
                return BadRequest("User does not exist");

            if (db.Card_Request.Any(r => r.UserId == dto.UserId && r.IsVerified == false))
                return BadRequest("You already have a pending request");

            var request = new Card_Request
            {
                UserId = dto.UserId,
                CardType = dto.CardType,
                BankName = dto.BankName.Trim(),
                AccountNumber = dto.AccountNumber.Trim(),
                IFSC_Code = dto.IFSC_Code.Trim(),
                AadhaarNumber = dto.AadhaarNumber.Trim(),
                IsVerified = false
            };

            db.Card_Request.Add(request);
            db.SaveChanges();

            dto.Card_Id = request.Card_Id;

            // Determine joining fee (C#7 compatible)
            decimal fee;
            switch (dto.CardType)
            {
                case "Gold":
                    fee = 1000;
                    break;
                case "Diamond":
                    fee = 2000;
                    break;
                default:
                    fee = 3000;
                    break;
            }

            var joiningFee = new Joining_Fee
            {
                Card_Id = request.Card_Id,
                PaymentMethodId = null,
                Amount = fee,
                Status = "Pending"
            };
            db.Joining_Fee.Add(joiningFee);
            db.SaveChanges();

            return Ok(new { message = "Card request submitted successfully. Pay joining fee.", dto });
        }

        // 3️⃣ Upload documents
        [HttpPost]
        [Route("upload-documents/{cardId}")]
        public async Task<IHttpActionResult> UploadDocuments(int cardId)
        {
            var request = db.Card_Request.Find(cardId);
            if (request == null)
                return BadRequest("Invalid Card Request ID");

            if (!HttpContext.Current.Request.Files.AllKeys.Any())
                return BadRequest("No files received");

            var allowedTypes = new[] { "Aadhaar", "PAN", "BankBook" };
            var uploadedDocs = new List<EmiCard_Documents>();

            foreach (string key in HttpContext.Current.Request.Files)
            {
                var file = HttpContext.Current.Request.Files[key];
                if (file == null || file.ContentLength == 0) continue;
                if (!allowedTypes.Contains(key)) continue;

                var fileName = Path.GetFileName(file.FileName);
                var serverPath = HttpContext.Current.Server.MapPath("~/Content/EmiCardDocuments/");
                Directory.CreateDirectory(serverPath);
                var fullPath = Path.Combine(serverPath, fileName);
                file.SaveAs(fullPath);

                var doc = new EmiCard_Documents
                {
                    Card_Id = cardId,
                    DocumentType = key,
                    FileName = fileName,
                    FilePath = "/Content/EmiCardDocuments/" + fileName,
                    UploadedDate = DateTime.Now
                };

                db.EmiCard_Documents.Add(doc);
                uploadedDocs.Add(doc);
            }

            await db.SaveChangesAsync();
            return Ok(new { message = "Documents uploaded successfully", documents = uploadedDocs });
        }

        // 4️⃣ Pay Joining Fee
        [HttpPost]
        [Route("payfee/{cardId}")]
        public IHttpActionResult PayJoiningFee(int cardId, [FromBody] PaymentDto payment)
        {
            if (payment == null)
                return BadRequest("Payment data is required.");

            // Find joining fee
            var joiningFee = db.Joining_Fee.FirstOrDefault(j => j.Card_Id == cardId);
            if (joiningFee == null)
                return NotFound();

            // Mark as paid
            joiningFee.Status = "Paid";
            joiningFee.PaymentMethodId = payment.PaymentMethodId;
            joiningFee.Amount = payment.Amount;

            db.SaveChanges();

            return Ok(new { message = "Payment successful. Await admin approval to activate EMI card." });
        }
    }
}
