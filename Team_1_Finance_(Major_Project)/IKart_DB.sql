Create Database IKart;

Use IKart;

-- 1. Admins
Create Table Admins (
    AdminId int primary key identity(1,1),
    Username nvarchar(50) not null unique,
    PasswordHash nvarchar(255) not null
);

-- 2. Payment Methods
Create Table PaymentMethods (
    PaymentMethodId int primary key identity(1,1),
    MethodName nvarchar(50)
);

-- 3. Users
Create Table Users (
    UserId int primary key identity(1,1),
    FullName nvarchar(100) not null,
    Email nvarchar(100) unique not null,
    PhoneNo nvarchar(15) unique not null,
    Username nvarchar(50) unique not null,
    PasswordHash nvarchar(255) not null,
    Otp nvarchar(10),
    Status nvarchar(20),
    CreatedDate datetime default getdate(),
    OtpExpiry datetime null,
    IsVerified bit null
);

-- 4. Address
Create Table Address (
    AddressId int primary key identity(1,1),
    UserId int foreign key references Users(UserId),
    Street nvarchar(100),
    City nvarchar(50),
    State nvarchar(50),
    ZipCode nvarchar(10),
    Country nvarchar(50)
);

-- 5. Stocks
Create Table Stocks (
    StockId int primary key identity(1,1),
    CategoryName nvarchar(50),
    SubCategoryName nvarchar(50),
    TotalStocks int,
    AvailableStocks int,
    LastUpdated datetime default getdate(),
    unique (CategoryName, SubCategoryName)
);

-- 6. Products
Create Table Products (
    ProductId int primary key identity(1,1),
    StockId int foreign key references Stocks(StockId),
    ProductName nvarchar(100),
    Cost decimal(10,2),
    CreatedDate datetime default getdate(),
    ProductDetails nvarchar(max),
    ProductImage nvarchar(max) null
);

-- 7. Payment Details
Create Table PaymentDetails (
    PaymentDetailsId int primary key identity(1,1),
    PaymentMethodId int foreign key references PaymentMethods(PaymentMethodId),
    CardNumber nvarchar(20),
    BankName nvarchar(50),
    ExpiryDate date,
    Cvv nvarchar(5)
);

-- 8. Card Request
Create Table CardRequest (
    CardId int primary key identity(1,1),
    UserId int foreign key references Users(UserId),
    CardType nvarchar(20),
    BankName nvarchar(50),
    AccountNumber nvarchar(30),
    IfscCode nvarchar(15),
    AadhaarNumber nvarchar(15),
    IsVerified bit default 0,
    VerifiedBy int foreign key references Admins(AdminId)
);

-- 9. EMI Card
Create Table EmiCard (
    EmiCardId int primary key identity(1,1),
    UserId int foreign key references Users(UserId),
    PaymentMethodId int foreign key references PaymentMethods(PaymentMethodId),
    ActivatedBy int foreign key references Admins(AdminId),
    CardType nvarchar(20),
    CardNumber nvarchar(20) unique,
    TotalLimit decimal(10,2),
    Balance decimal(10,2),
    IsActive bit default 0,
    IssueDate date,
    ExpireDate date,
    CardImage nvarchar(max) null
);

-- 10. Payments
Create Table Payments (
    PaymentId int primary key identity(1,1),
    EmiCardId int null foreign key references EmiCard(EmiCardId),
    UserId int not null foreign key references Users(UserId),
    ProductId int not null foreign key references Products(ProductId),
    PaymentMethodId int not null foreign key references PaymentMethods(PaymentMethodId),
    ProcessingFee decimal(18,2) default 0,
    TotalAmount decimal(18,2) not null,
    RazorpayPaymentId nvarchar(100) null,
    PaymentDate datetime default getdate(),
    Status nvarchar(50) default 'Pending'
);

-- 11. Orders
Create Table Orders (
    OrderId int primary key identity(1,1),
    ProductId int foreign key references Products(ProductId),
    UserId int foreign key references Users(UserId),
    PaymentId int foreign key references Payments(PaymentId),
    OrderDate datetime default getdate(),
    DeliveryDate datetime,
    Region nvarchar(50) null,
    OrderStatus nvarchar(50) null
);

-- 12. Joining Fee
Create Table JoiningFee (
    FeeId int primary key identity(1,1),
    CardId int foreign key references CardRequest(CardId),
    PaymentMethodId int foreign key references PaymentMethods(PaymentMethodId),
    Amount decimal(10,2),
    Status nvarchar(20),
    RazorpayPaymentId nvarchar(100) null,
    PaymentId nvarchar(100) null
);

-- 13. Monthly EMI Calc
Create Table MonthlyEmiCalc (
    EmiId int primary key identity(1,1),
    PaymentId int foreign key references Payments(PaymentId),
    UserId int foreign key references Users(UserId),
    TotalAmount decimal(10,2),
    EmiAmount decimal(10,2),
    TenureMonths int,
    RemainingAmount decimal(10,2)
);

-- 14. Installment Payments
Create Table InstallmentPayments (
    InstallmentId int primary key identity(1,1),
    EmiId int foreign key references MonthlyEmiCalc(EmiId),
    DueDate date,
    Amount decimal(10,2),
    IsPaid bit default 0,
    PaymentId int null foreign key references Payments(PaymentId)
);

-- 15. COD UPI Orders
Create Table CodUpiOrders (
    OrderId int primary key identity(1000,1),
    ProductId int foreign key references Products(ProductId),
    UserId int foreign key references Users(UserId),
    OrderDate datetime not null default getdate(),
    DeliveryDate datetime not null,
    PaymentType varchar(10) not null check (PaymentType in ('COD', 'UPI')),
    PaymentStatus varchar(20) not null default 'Pending'
);

-- 16. Penalty
Create Table Penalty (
    PenaltyId int primary key identity(1,1),
    InstallmentId int foreign key references InstallmentPayments(InstallmentId),
    UserId int foreign key references Users(UserId),
    DueDate date,
    DaysOverdue int,
    PenaltyPerDay decimal(10,2) default 50,
    PenaltyAmount decimal(10,2),
    Status nvarchar(20),
    LastUpdated datetime default getdate()
);

-- 17. FAQ
Create Table Faq (
    FaqId int primary key identity(1,1),
    ProductId int foreign key references Products(ProductId),
    Question nvarchar(255),
    Answer nvarchar(max),
    CreatedDate datetime default getdate()
);

-- 18. Refunds
Create Table Refunds (
    RefundId int primary key identity(1,1),
    PaymentId int foreign key references Payments(PaymentId),
    Amount decimal(10,2),
    Reason nvarchar(255),
    Status nvarchar(20),
    RefundDate datetime default getdate()
);

-- 19. Support Tickets
Create Table SupportTickets (
    TicketId int primary key identity(1,1),
    UserId int foreign key references Users(UserId),
    Subject nvarchar(100),
    Description nvarchar(max),
    Status nvarchar(20),
    CreatedDate datetime default getdate(),
    ClosedDate datetime
);

-- 20. EMI Card Documents
Create Table EmiCardDocuments (
    DocumentId int primary key identity(1,1),
    CardId int foreign key references CardRequest(CardId),
    DocumentType nvarchar(50),
    FileName nvarchar(255),
    FilePath nvarchar(max),
    UploadedDate datetime default getdate()
);

-- 21. Returns
Create Table Returns (
    ReturnId int primary key identity(1,1),
    OrderId int foreign key references Orders(OrderId),
    UserId int foreign key references Users(UserId),
    ProductId int foreign key references Products(ProductId),
    Reason nvarchar(255),
    Status nvarchar(50),
    ReturnDate datetime default getdate()
);

-- 22. Order Cancellations
Create Table OrderCancellations (
    CancellationId int primary key identity(1,1),
    OrderId int foreign key references Orders(OrderId),
    UserId int foreign key references Users(UserId),
    Reason nvarchar(255),
    CancelDate datetime default getdate(),
    Refunded bit default 0
);


insert into Admins values('Ikart','admin')

insert into payment_methods (methodname) values ('credit');
insert into payment_methods (methodname) values ('debit');
insert into payment_methods (methodname) values ('upi');
insert into payment_methods (methodname) values ('razorpay');
insert into payment_methods (methodname) values ('card');
