CREATE TABLE Payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReservationID INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Description VARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign key constraints
    CONSTRAINT FK_Payments_Reservation FOREIGN KEY (ReservationId) REFERENCES Reservations (Id),
);
