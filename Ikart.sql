DROP DATABASE IKart;

CREATE DATABASE IKart;
USE IKart;

-- 1. Admins
CREATE TABLE Admins (
    AdminId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL
);

-- 2. Users
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PhoneNo NVARCHAR(15) UNIQUE NOT NULL,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    OTP NVARCHAR(10),
    Status NVARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 3. Address
CREATE TABLE Address (
    AddressId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    Street NVARCHAR(100),
    City NVARCHAR(50),
    State NVARCHAR(50),
    ZipCode NVARCHAR(10),
    Country NVARCHAR(50)
);

-- 4. Stocks
CREATE TABLE Stocks (
    Stock_Id INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(50),
    SubCategoryName NVARCHAR(50),
    Total_Stocks INT,
    Available_Stocks INT,
    Last_Updated DATETIME DEFAULT GETDATE(),
    UNIQUE(CategoryName, SubCategoryName)
);

-- 5. Products
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    Stock_Id INT FOREIGN KEY REFERENCES Stocks(Stock_Id),
    ProductName NVARCHAR(100),
    Cost DECIMAL(10,2),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ProductDetails NVARCHAR(MAX),
    ProductImage NVARCHAR(MAX) NULL
);

-- 6. Payment Methods
CREATE TABLE Payment_Methods (
    PaymentMethodId INT PRIMARY KEY IDENTITY(1,1),
    MethodName NVARCHAR(50)
);

-- 7. Payment Details
CREATE TABLE Payment_Details (
    PaymentDetailsId INT PRIMARY KEY IDENTITY(1,1),
    PaymentMethodId INT FOREIGN KEY REFERENCES Payment_Methods(PaymentMethodId),
    CardNumber NVARCHAR(20),
    BankName NVARCHAR(50),
    ExpiryDate DATE,
    CVV NVARCHAR(5)
);

-- 8. EMI Card
CREATE TABLE EMI_Card (
    EmiCardId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    PaymentMethodId INT FOREIGN KEY REFERENCES Payment_Methods(PaymentMethodId),
    ActivatedBy INT FOREIGN KEY REFERENCES Admins(AdminId),
    CardType NVARCHAR(20),
    CardNumber NVARCHAR(20) UNIQUE,
    TotalLimit DECIMAL(10,2),
    Balance DECIMAL(10,2),
    IsActive BIT DEFAULT 0,
    IssueDate DATE,
    ExpireDate DATE,
    CardImage NVARCHAR(MAX) NULL
);

-- 9. Payments
CREATE TABLE Payments (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    EmiCardId INT FOREIGN KEY REFERENCES EMI_Card(EmiCardId),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    PaymentMethodId INT FOREIGN KEY REFERENCES Payment_Methods(PaymentMethodId),
    ProcessingFee DECIMAL(10,2),
    TotalAmount DECIMAL(10,2),
    PaymentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) CHECK (Status IN ('Pending','Paid','Failed'))
);

-- 10. Orders
CREATE TABLE Orders (
    Order_Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    PaymentId INT FOREIGN KEY REFERENCES Payments(PaymentId),
    OrderDate DATETIME DEFAULT GETDATE(),
    DeliveryDate DATETIME
);

-- 11. Card Request
CREATE TABLE Card_Request (
    Card_Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    CardType NVARCHAR(20),
    BankName NVARCHAR(50),
    AccountNumber NVARCHAR(30),
    IFSC_Code NVARCHAR(15),
    AadhaarNumber NVARCHAR(15),
    IsVerified BIT DEFAULT 0,
    VerifiedBy INT FOREIGN KEY REFERENCES Admins(AdminId)
);

-- 12. Joining Fee
CREATE TABLE Joining_Fee (
    Fee_Id INT PRIMARY KEY IDENTITY(1,1),
    Card_Id INT FOREIGN KEY REFERENCES Card_Request(Card_Id),
    PaymentMethodId INT FOREIGN KEY REFERENCES Payment_Methods(PaymentMethodId),
    Amount DECIMAL(10,2),
    Status NVARCHAR(20)
);

-- 13. Monthly EMI Calc
CREATE TABLE Monthly_EMI_Calc (
    EMI_Id INT PRIMARY KEY IDENTITY(1,1),
    PaymentId INT FOREIGN KEY REFERENCES Payments(PaymentId),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    TotalAmount DECIMAL(10,2),
    EMIAmount DECIMAL(10,2),
    TenureMonths INT,
    RemainingAmount DECIMAL(10,2)
);

-- 14. Installment Payments
CREATE TABLE Installment_Payments (
    InstallmentId INT PRIMARY KEY IDENTITY(1,1),
    EMI_Id INT FOREIGN KEY REFERENCES Monthly_EMI_Calc(EMI_Id),
    DueDate DATE,
    Amount DECIMAL(10,2),
    IsPaid BIT DEFAULT 0
);

-- 15. Penalty
CREATE TABLE Penalty (
    PenaltyId INT PRIMARY KEY IDENTITY(1,1),
    InstallmentId INT FOREIGN KEY REFERENCES Installment_Payments(InstallmentId),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    DueDate DATE,
    Days_Overdue INT,
    PenaltyPerDay DECIMAL(10,2) DEFAULT 50,
    PenaltyAmount DECIMAL(10,2),
    Status NVARCHAR(20),
    LastUpdated DATETIME DEFAULT GETDATE()
);

-- 16. FAQ
CREATE TABLE FAQ (
    FaqId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    Question NVARCHAR(255),
    Answer NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 17. Offers
CREATE TABLE Offers (
    OfferId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    DiscountPercent DECIMAL(5,2),
    ValidFrom DATE,
    ValidTo DATE,
    Terms NVARCHAR(MAX),
    CONSTRAINT chk_OfferDates CHECK (ValidFrom < ValidTo)
);

-- 18. Refunds
CREATE TABLE Refunds (
    RefundId INT PRIMARY KEY IDENTITY(1,1),
    PaymentId INT FOREIGN KEY REFERENCES Payments(PaymentId),
    Amount DECIMAL(10,2),
    Reason NVARCHAR(255),
    Status NVARCHAR(20),
    RefundDate DATETIME DEFAULT GETDATE()
);

-- 19. Support Tickets
CREATE TABLE Support_Tickets (
    TicketId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    Subject NVARCHAR(100),
    Description NVARCHAR(MAX),
    Status NVARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ClosedDate DATETIME
);

--20. EMI Card Documents
-- New table for EMI Card Documents
CREATE TABLE EmiCard_Documents (
    DocumentId INT PRIMARY KEY IDENTITY(1,1),
    Card_Id INT FOREIGN KEY REFERENCES Card_Request(Card_Id),
    DocumentType NVARCHAR(50), -- Aadhaar / AddressProof / FeeReceipt
    FileName NVARCHAR(255),    -- stored name in project folder
    FilePath NVARCHAR(MAX),    -- actual local path
    UploadedDate DATETIME DEFAULT GETDATE()
);

-- Insert Stocks
INSERT INTO Stocks (CategoryName, SubCategoryName, Total_Stocks, Available_Stocks) VALUES 
('Electronics', 'Smartphones', 100, 90),
('Home Appliances', 'Washing Machines', 50, 45),
('Fashion', 'Footwear', 200, 180);

-- Insert Products
INSERT INTO Products (Stock_Id, ProductName, Cost, ProductDetails, ProductImage) VALUES 
(1, 'Samsung Galaxy S23', 799.99, 'Latest Android flagship with AMOLED display', 's23.jpg'),
(2, 'Bosch Front Load Washer', 499.50, 'Energy-efficient washing machine with smart features', 'bosch_washer.jpg'),
(3, 'Nike Air Max', 129.99, 'Comfortable and stylish running shoes', 'airmax.jpg');

-- Insert Users
INSERT INTO Users (FullName, Email, PhoneNo, Username, PasswordHash, OTP, Status) VALUES
('Arun Kumar', 'arun.kumar@example.com', '9876543210', 'arun123', 'hashedpwd1', '123456', 'Active'),
('Priya Sharma', 'priya.sharma@example.com', '9123456780', 'priyaS', 'hashedpwd2', NULL, 'Active'),
('Vikram Singh', 'vikram.singh@example.com', '9001122334', 'vikramS', 'hashedpwd3', NULL, 'Inactive');

-- Insert Payment Methods
INSERT INTO Payment_Methods (MethodName) VALUES
('Credit Card'),
('Debit Card'),
('Net Banking'),
('UPI');

-- Insert Admins
INSERT INTO Admins (Username, PasswordHash) VALUES
('superadmin', 'adminhashed1'),
('manager1', 'adminhashed2');

-- Insert Card Requests
INSERT INTO Card_Request (UserId, CardType, BankName, AccountNumber, IFSC_Code, AadhaarNumber, VerifiedBy) VALUES
(1, 'Gold', 'HDFC Bank', '123456789012', 'HDFC0001234', '123412341234', NULL),
(2, 'Platinum', 'ICICI Bank', '987654321098', 'ICIC0005678', '567856785678', NULL),
(3, 'Silver', 'SBI Bank', '111122223333', 'SBIN0009876', '999988887777', NULL);

-- Insert EMI Cards
INSERT INTO EMI_Card (UserId, PaymentMethodId, ActivatedBy, CardType, CardNumber, TotalLimit, Balance, IsActive, IssueDate, ExpireDate, CardImage) VALUES
(1, 1, 1, 'Gold', '4111111111111111', 100000, 95000, 1, '2025-01-01', '2030-01-01', 'gold_card.png'),
(2, 2, 2, 'Platinum', '4222222222222222', 200000, 180000, 1, '2025-02-01', '2030-02-01', 'platinum_card.png');

-- Insert Payments
INSERT INTO Payments (EmiCardId, UserId, ProductId, PaymentMethodId, ProcessingFee, TotalAmount, Status) VALUES
(1, 1, 1, 1, 15.00, 814.99, 'Paid'),
(2, 2, 2, 2, 20.00, 519.50, 'Pending'),
(NULL, 3, 3, 4, 5.00, 134.99, 'Failed');

-- Insert Orders
INSERT INTO Orders (ProductId, UserId, PaymentId, DeliveryDate) VALUES
(1, 1, 1, '2025-09-10'),
(2, 2, 2, '2025-09-12'),
(3, 3, 3, NULL);

-- EMI Plans for Orders (only valid PaymentIds)

INSERT INTO Monthly_EMI_Calc (PaymentId, UserId, TotalAmount, EMIAmount, TenureMonths, RemainingAmount)
VALUES
(1, 1, 814.99, 271.66, 3, 0.00),         -- Arun Kumar (Completed, all EMIs paid)
(2, 2, 519.50, 173.17, 3, 346.33);       -- Priya Sharma (Pending, 1 paid, 2 left)
 
-- Installments for Arun Kumar (Paid)
INSERT INTO Installment_Payments (EMI_Id, DueDate, Amount, IsPaid)
VALUES
(1, '2025-09-02', 271.66, 1),
(1, '2025-10-02', 271.66, 1),
(1, '2025-11-02', 271.67, 1);

-- Installments for Priya Sharma (Pending)
INSERT INTO Installment_Payments (EMI_Id, DueDate, Amount, IsPaid)
VALUES
(2, '2025-09-01', 173.17, 1),  -- Paid first installment
(2, '2025-10-01', 173.17, 0),  -- Pending
(2, '2025-11-01', 173.16, 0);  -- Pending
 
 --Inserting  User Docs
INSERT INTO EmiCard_Documents (Card_Id, DocumentType, FileName, FilePath)
VALUES 
(1, 'Aadhar Card', 'aadhar1.pdf', '/Content/EmiCardDocs/aadhar1.pdf'),
(1, 'PAN Card', 'pan1.jpg', '/Content/EmiCardDocs/pan1.jpg'),
(1, 'Bank Statement', 'bank1.docx', '/Content/EmiCardDocs/bank1.docx');

select * from EmiCard_Documents

SELECT Card_Id, Status
FROM Joining_Fee;

select * from Card_Request