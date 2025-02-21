CREATE PROCEDURE InsertAuditLog
    @Activity VARCHAR(255),
    @UserId INT,
    @Status VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AuditLogs (Activity, UserId, Status, CreatedDate)
    VALUES (@Activity, @UserId, @Status, GETDATE());
END;
