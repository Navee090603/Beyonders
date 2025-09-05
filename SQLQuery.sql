create database FinalProjectDB
use FinalProjectDB

CREATE TABLE Admins (
    AdminId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL
);

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

CREATE TABLE Address (
    AddressId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    Street NVARCHAR(100),
    City NVARCHAR(50),
    State NVARCHAR(50),
    ZipCode NVARCHAR(10),
    Country NVARCHAR(50)
);

CREATE TABLE Stocks (
    Stock_Id INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(50),
    Total_Stocks INT,
    Available_Stocks INT,
    Last_Updated DATETIME DEFAULT GETDATE()
);

CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    Stock_Id INT FOREIGN KEY REFERENCES Stocks(Stock_Id),
    ProductName NVARCHAR(100),
    Cost DECIMAL(10,2),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ProductDetails NVARCHAR(MAX)
);

CREATE TABLE Orders (
    Order_Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    PaymentId INT,
    OrderDate DATETIME DEFAULT GETDATE(),
    DeliveryDate DATETIME
);

CREATE TABLE Payments (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    EmiCardId INT,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    PaymentMethodId INT,
    ProcessingFee DECIMAL(10,2),
    TotalAmount DECIMAL(10,2),
    PaymentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20)
);

CREATE TABLE Payment_Methods (
    PaymentMethodId INT PRIMARY KEY IDENTITY(1,1),
    MethodName NVARCHAR(50) -- e.g. Debit Card, NetBanking, UPI
);

CREATE TABLE Payment_Details (
    PaymentDetailsId INT PRIMARY KEY IDENTITY(1,1),
    PaymentMethodId INT FOREIGN KEY REFERENCES Payment_Methods(PaymentMethodId),
    CardNumber NVARCHAR(20),
    BankName NVARCHAR(50),
    ExpiryDate DATE,
    CVV NVARCHAR(5)
);

CREATE TABLE EMI_Card (
    EmiCardId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    PaymentMethodId INT,
    ActivatedBy INT FOREIGN KEY REFERENCES Admins(AdminId),
    CardType NVARCHAR(20),
    CardNumber NVARCHAR(20) UNIQUE,
    TotalLimit DECIMAL(10,2),
    Balance DECIMAL(10,2),
    IsActive BIT DEFAULT 0,
    IssueDate DATE,
    ExpireDate DATE
);

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

CREATE TABLE Joining_Fee (
    Fee_Id INT PRIMARY KEY IDENTITY(1,1),
    Card_Id INT FOREIGN KEY REFERENCES Card_Request(Card_Id),
    PaymentMethodId INT FOREIGN KEY REFERENCES Payment_Methods(PaymentMethodId),
    Amount DECIMAL(10,2),
    Status NVARCHAR(20)
);

CREATE TABLE Monthly_EMI_Calc (
    EMI_Id INT PRIMARY KEY IDENTITY(1,1),
    PaymentId INT FOREIGN KEY REFERENCES Payments(PaymentId),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    TotalAmount DECIMAL(10,2),
    EMIAmount DECIMAL(10,2),
    TenureMonths INT,
    RemainingAmount DECIMAL(10,2)
);

CREATE TABLE Installment_Payments (
    InstallmentId INT PRIMARY KEY IDENTITY(1,1),
    EMI_Id INT FOREIGN KEY REFERENCES Monthly_EMI_Calc(EMI_Id),
    DueDate DATE,
    Amount DECIMAL(10,2),
    IsPaid BIT DEFAULT 0
);

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

CREATE TABLE FAQ (
    FaqId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    Question NVARCHAR(255),
    Answer NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Offers (
    OfferId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    DiscountPercent DECIMAL(5,2),
    ValidFrom DATE,
    ValidTo DATE,
    Terms NVARCHAR(MAX)
);

CREATE TABLE Refunds (
    RefundId INT PRIMARY KEY IDENTITY(1,1),
    PaymentId INT FOREIGN KEY REFERENCES Payments(PaymentId),
    Amount DECIMAL(10,2),
    Reason NVARCHAR(255),
    Status NVARCHAR(20),
    RefundDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Support_Tickets (
    TicketId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    Subject NVARCHAR(100),
    Description NVARCHAR(MAX),
    Status NVARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ClosedDate DATETIME
);