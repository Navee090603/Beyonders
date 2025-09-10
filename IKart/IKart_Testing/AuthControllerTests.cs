using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using IKart_ServerSide.Controllers.Users;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Authentication;
using System.Web.Http.Results;

namespace IKart_Testing
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<DbSet<User>> CreateMockUserDbSet(List<User> users)
        {
            var queryable = users.AsQueryable();

            var mockSet = new Mock<DbSet<User>>();
            mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }

        [TestMethod]
        public void Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var dto = new UserLoginDto
            {
                Username = "Nikitha_229",
                Password = "Nikitha@123"
            };

            var testUser = new User
            {
                UserId = 4,
                Username = "Nikitha_229",
                PasswordHash = "Nikitha@123",
                FullName = "Nikitha Tejaswini",
                IsVerified = true
            };

            var mockUsers = CreateMockUserDbSet(new List<User> { testUser });

            var mockDb = new Mock<IKartEntities>();
            mockDb.Setup(db => db.Users).Returns(mockUsers.Object);

            var controller = new AuthController(mockDb.Object);

            // Act
            var result = controller.Login(dto);

            // Assert
            var okResult = result as OkNegotiatedContentResult<LoginResponseDto>;
            Assert.IsNotNull(okResult, $"Expected OkNegotiatedContentResult<LoginResponseDto> but got {result.GetType().Name}");

            var content = okResult.Content;
            Assert.AreEqual("Login successful", content.Message);
            Assert.AreEqual(4, content.UserId);
            Assert.AreEqual("Nikitha Tejaswini", content.FullName);
            Assert.AreEqual("Nikitha_229", content.Username);
        }
        [TestMethod]
        public void Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new UserLoginDto
            {
                Username = "WrongUser",
                Password = "WrongPass"
            };

            var testUser = new User
            {
                UserId = 1,
                Username = "Nikitha_29",
                PasswordHash = "Nikitha@123",
                FullName = "Nikitha Tejaswini",
                IsVerified = true
            };

            var mockUsers = CreateMockUserDbSet(new List<User> { testUser });

            var mockDb = new Mock<IKartEntities>();
            mockDb.Setup(db => db.Users).Returns(mockUsers.Object);

            var controller = new AuthController(mockDb.Object);

            // Act
            var result = controller.Login(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        
    }
}
