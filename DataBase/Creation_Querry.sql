CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PhoneNo VARCHAR(15) NOT NULL UNIQUE,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    OTP VARCHAR(10) UNIQUE,
    Status VARCHAR(10) NOT NULL CHECK (Status IN ('Active', 'Deactive')),
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Admin (
    AdminId INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL
);

CREATE TABLE Stocks (
    Stock_Id INT PRIMARY KEY IDENTITY(1,1),
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    Total_Stocks INT NOT NULL,
    Available_Stocks INT NOT NULL,
    Last_Updated DATETIME DEFAULT GETDATE()
);

CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    Stock_Id INT NOT NULL,
    ProductName VARCHAR(100) NOT NULL,
    Cost DECIMAL(10, 2) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Stock_Id) REFERENCES Stocks(Stock_Id)
);

CREATE TABLE Offers (
    OfferId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    DiscountPercent DECIMAL(5, 2) NOT NULL,
    ValidFrom DATE NOT NULL,
    ValidTo DATE NOT NULL,
    Terms TEXT NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE FAQ (
    FAQId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    Question TEXT NOT NULL,
    Answer TEXT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE Card_Request (
    Card_Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Card_Type VARCHAR(50) NOT NULL,
    BankName VARCHAR(100) NOT NULL,
    AccountNumber VARCHAR(30) NOT NULL,
    IFSC_Code VARCHAR(20) NOT NULL,
    AadharNumber VARCHAR(20) NOT NULL,
    Aadhar_Image TEXT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE Payment_Methods (
    PaymentMethodId INT PRIMARY KEY IDENTITY(1,1),
    MethodName VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE EMI_Card (
    EMICardId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    PaymentMethodId INT NOT NULL,
    CardType VARCHAR(50) NOT NULL,
    CardNumber VARCHAR(30) NOT NULL UNIQUE,
    TotalLimit DECIMAL(10,2) NOT NULL,
    Balance DECIMAL(10,2) NOT NULL,
    ValidTill DATE NOT NULL,
    IsActive BIT NOT NULL,
    ActivatedDate DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (PaymentMethodId) REFERENCES Payment_Methods(PaymentMethodId)
);

CREATE TABLE Card_Transactions (
    TxnId INT PRIMARY KEY IDENTITY(1,1),
    EMICardId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Type VARCHAR(10) NOT NULL CHECK (Type IN ('Credit', 'Debit')),
    Date DATETIME NOT NULL,
    Remarks TEXT,
    FOREIGN KEY (EMICardId) REFERENCES EMI_Card(EMICardId)
);

CREATE TABLE Address (
    AddressId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Street VARCHAR(255) NOT NULL,
    City VARCHAR(100) NOT NULL,
    State VARCHAR(100) NOT NULL,
    ZipCode VARCHAR(10) NOT NULL,
    Country VARCHAR(100) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE Payments (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    PaymentMethodId INT NOT NULL,
    ProductAmount DECIMAL(10,2) NOT NULL,
    PlatformFee DECIMAL(10,2) DEFAULT 0.00,
    ProcessingFee DECIMAL(10,2) DEFAULT 0.00,
    TotalAmount AS (ProductAmount + PlatformFee + ProcessingFee) PERSISTED,
    PaymentDate DATETIME NOT NULL,
    Status VARCHAR(20) NOT NULL CHECK (Status IN ('Completed', 'Pending', 'Failed')),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    FOREIGN KEY (PaymentMethodId) REFERENCES Payment_Methods(PaymentMethodId)
);

CREATE TABLE Monthly_EMI_Calc (
    EMIId INT PRIMARY KEY IDENTITY(1,1),
    PaymentId INT NOT NULL,
    EMICardId INT NULL,
    UserId INT NOT NULL,
    EMIOption INT NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    EMIAmount DECIMAL(10,2) NOT NULL,
    RemainingAmount DECIMAL(10,2) NOT NULL,
    Status VARCHAR(20) NOT NULL CHECK (Status IN ('Ongoing', 'Completed', 'Defaulted')),
    FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId),
    FOREIGN KEY (EMICardId) REFERENCES EMI_Card(EMICardId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE Installment_Payments (
    InstallmentId INT PRIMARY KEY IDENTITY(1,1),
    EMIId INT NOT NULL,
    DueDate DATE NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    IsPaid BIT NOT NULL,
    FOREIGN KEY (EMIId) REFERENCES Monthly_EMI_Calc(EMIId)
);

CREATE TABLE Support_Tickets (
    TicketId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Subject VARCHAR(255) NOT NULL,
    Description TEXT NOT NULL,
    Status VARCHAR(20) NOT NULL CHECK (Status IN ('Open', 'Closed', 'In Progress')),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE Orders (
    Order_Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    PaymentId INT NOT NULL,
    OrderPlacedDate DATETIME NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId)
);