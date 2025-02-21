CREATE TABLE AuditLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Activity VARCHAR(255) NULL,
	UserId INT NULL,
    Status VARCHAR(50) NOT NULL CHECK (Status IN ('Success', 'Failed')),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
    
    -- Foreign key constraints
    CONSTRAINT FK_AuditLog_User FOREIGN KEY (UserId) REFERENCES Users (Id)
);