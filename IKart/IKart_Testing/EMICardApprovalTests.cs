using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using IKart_ServerSide.Models;

namespace IKart_Testing
{
    [TestClass]
    public class EMICardApprovalTests
    {
        private Mock<DbSet<Card_Request>> CreateMockCardRequestDbSet(List<Card_Request> requests)
        {
            var queryable = requests.AsQueryable();

            var mockSet = new Mock<DbSet<Card_Request>>();
            mockSet.As<IQueryable<Card_Request>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Card_Request>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Card_Request>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Card_Request>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }

        [TestMethod]
        public void AdminApprovedCardRequest_ReturnsTrue()
        {
            // Arrange
            var userId = 101;
            var approvedRequest = new Card_Request
            {
                Card_Id = 1,
                UserId = userId,
                IsVerified = true
            };

            var mockRequests = CreateMockCardRequestDbSet(new List<Card_Request> { approvedRequest });

            var mockDb = new Mock<IKartEntities>();
            mockDb.Setup(db => db.Card_Request).Returns(mockRequests.Object);

            // Act
            var isApproved = mockDb.Object.Card_Request.Any(r => r.UserId == userId && r.IsVerified == true);

            // Assert
            Assert.IsTrue(isApproved, "Expected the EMI card request to be approved.");
        }

        [TestMethod]
        public void AdminNotApprovedCardRequest_ReturnsFalse()
        {
            // Arrange
            var userId = 102;
            var pendingRequest = new Card_Request
            {
                Card_Id = 2,
                UserId = userId,
                IsVerified = false
            };

            var mockRequests = CreateMockCardRequestDbSet(new List<Card_Request> { pendingRequest });

            var mockDb = new Mock<IKartEntities>();
            mockDb.Setup(db => db.Card_Request).Returns(mockRequests.Object);

            // Act
            var isApproved = mockDb.Object.Card_Request.Any(r => r.UserId == userId && r.IsVerified == true);

            // Assert
            Assert.IsFalse(isApproved, "Expected the EMI card request to be pending (not approved).");
        }
    }
}
