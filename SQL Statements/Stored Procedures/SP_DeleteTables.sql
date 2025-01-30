CREATE PROCEDURE DeleteTable
    @Id INT
AS
BEGIN
    UPDATE Tables
    SET IsDeleted = 1,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;
END;
