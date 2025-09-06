using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;
using IKart_Shared.DTOs.EMI_Card;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/emicards")]
    public class EMICardsController : ApiController
    {
        IKartEntities db = new IKartEntities();

        // ✅ Get all EMI cards for a user
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

        // ✅ Request new card
        [HttpPost]
        [Route("request")]
        public IHttpActionResult RequestCard(CardRequestDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)));

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
            return Ok(new { message = "Card request submitted successfully", dto });
        }

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
                if (file == null || file.ContentLength == 0)
                    continue;

                var docType = key; // Expecting keys: Aadhaar, PAN, BankBook
                if (!allowedTypes.Contains(docType))
                    continue;

                var fileName = Path.GetFileName(file.FileName);
                var serverPath = HttpContext.Current.Server.MapPath("~/Uploads/EMIDocs/");
                Directory.CreateDirectory(serverPath); // Ensure folder exists

                var fullPath = Path.Combine(serverPath, fileName);
                file.SaveAs(fullPath);

                var doc = new EmiCard_Documents
                {
                    Card_Id = cardId,
                    DocumentType = docType,
                    FileName = fileName,
                    FilePath = fullPath,
                    UploadedDate = DateTime.Now
                };

                db.EmiCard_Documents.Add(doc);
                uploadedDocs.Add(doc);
            }

            await db.SaveChangesAsync();
            return Ok(new { message = "Documents uploaded successfully", documents = uploadedDocs });
        }


        // ✅ After admin approval → activate EMI card
        [HttpPost]
        [Route("activate/{cardRequestId}")]
        public IHttpActionResult ActivateCard(int cardRequestId)
        {
            var req = db.Card_Request.Find(cardRequestId);
            if (req == null) return NotFound();
            if ((bool)!req.IsVerified) return BadRequest("Request not approved by admin yet");

            // Assign card limits & fees
            decimal limit, fee;
            if (req.CardType == "Gold") { limit = 25000; fee = 1000; }
            else if (req.CardType == "Diamond") { limit = 50000; fee = 2000; }
            else { limit = 100000; fee = 3000; }

            var emiCard = new EMI_Card
            {
                UserId = req.UserId,
                CardType = req.CardType,
                CardNumber = "EMI" + DateTime.Now.Ticks.ToString().Substring(5, 10),
                TotalLimit = limit,
                Balance = limit,
                IsActive = false, // becomes active after payment
                IssueDate = DateTime.Now,
                ExpireDate = DateTime.Now.AddYears(5)
            };

            db.EMI_Card.Add(emiCard);

            // Add joining fee record
            var joiningFee = new Joining_Fee
            {
                Card_Id = req.Card_Id,
                PaymentMethodId = null,
                Amount = fee,
                Status = "Pending"
            };
            db.Joining_Fee.Add(joiningFee);

            db.SaveChanges();

            return Ok(new { message = "Card approved. Pay joining fee to activate.", emiCard });
        }

        // ✅ Pay joining fee → activate card
        [HttpPost]
        [Route("payfee/{feeId}")]
        public IHttpActionResult PayJoiningFee(int feeId)
        {
            var fee = db.Joining_Fee.Find(feeId);
            if (fee == null) return NotFound();

            fee.Status = "Paid";

            var emiCard = db.EMI_Card.FirstOrDefault(c => c.UserId == fee.Card_Request.UserId && c.CardType == fee.Card_Request.CardType);
            if (emiCard == null) return BadRequest("Associated EMI Card not found");

            emiCard.IsActive = true;
            db.SaveChanges();

            return Ok(new { message = "Joining fee paid successfully. Card activated.", emiCard });
        }
    }
}
