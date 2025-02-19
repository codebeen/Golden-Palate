CREATE PROCEDURE UpdateUserStatus
    @Id INT,
    @Status VARCHAR(50)
AS
BEGIN
    UPDATE Users
    SET Status = @Status,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;