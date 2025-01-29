CREATE TABLE Tables (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TableNumber INT NOT NULL,
    Description VARCHAR(MAX) NULL,
    SeatingCapacity INT NOT NULL,
    TableLocation VARCHAR(100) NOT NULL CHECK (TableLocation IN ('Window seat', 'Outdoor', 'Indoor')),
    Price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    TableImagePath VARCHAR(MAX) NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Available' CHECK (Status IN ('Available', 'Reserved', 'Occupied', 'Maintenance')),
    IsDeleted INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
