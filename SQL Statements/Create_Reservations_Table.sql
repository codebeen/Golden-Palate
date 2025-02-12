CREATE TABLE Reservations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	ReservationNumber VARCHAR(255) UNIQUE NOT NULL,
    ReservationDate DATE NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    BuffetType VARCHAR(255) NOT NULL,
    SpecialRequest VARCHAR(255) NULL,
    Status VARCHAR(50) DEFAULT 'Pending' NOT NULL CHECK (Status IN ('Pending', 'Ongoing', 'Completed', 'Cancelled')),
    TableId INT NOT NULL,
    CustomerId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign key constraints
    CONSTRAINT FK_Reservation_Table FOREIGN KEY (TableId) REFERENCES Tables (Id),
    CONSTRAINT FK_Reservation_Customer FOREIGN KEY (CustomerId) REFERENCES Customers (Id)
);
