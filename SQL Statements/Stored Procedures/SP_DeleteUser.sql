CREATE PROCEDURE DeleteUser
    @Id INT
AS
BEGIN
    UPDATE Users
    SET IsDeleted = 1,
		Status = 'Inactive',
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;
END;
