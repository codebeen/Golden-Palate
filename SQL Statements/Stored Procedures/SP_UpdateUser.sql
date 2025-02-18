CREATE PROCEDURE UpdateUser
    @Id INT,
    @Role VARCHAR(100)
AS
BEGIN
    UPDATE Users
    SET Role = @Role,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;
END;
