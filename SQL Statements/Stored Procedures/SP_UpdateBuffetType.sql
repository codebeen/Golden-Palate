CREATE PROCEDURE UpdateBuffetType
    @Id INT,
    @Name VARCHAR(100),
    @Description VARCHAR(255) = NULL
AS
BEGIN
    UPDATE BuffetTypes
    SET Name = @Name,
        Description = @Description,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;
END;
