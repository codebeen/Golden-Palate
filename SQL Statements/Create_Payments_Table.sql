CREATE TABLE Payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Amount DECIMAL(10,2) NOT NULL,
    Description VARCHAR(255) NULL,
	ReservationId INT NOT NULL,
    UserId INT NOT NULL,
    ModeOfPayment VARCHAR(50) NOT NULL CHECK (ModeOfPayment IN ('Cash', 'Gcash', 'Paymaya', 'Card')),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign key constraints
    CONSTRAINT FK_Payment_Reservation FOREIGN KEY (ReservationId) REFERENCES Reservations (Id),
    CONSTRAINT FK_Payment_User FOREIGN KEY (UserId) REFERENCES Users (Id)
);
