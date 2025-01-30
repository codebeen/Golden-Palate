CREATE PROCEDURE DeleteBuffetType
    @Id INT
AS
BEGIN
    UPDATE BuffetTypes
    SET IsDeleted = 1,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;
END;
