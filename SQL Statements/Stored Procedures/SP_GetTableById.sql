CREATE PROCEDURE GetTableById
    @Id INT
AS
BEGIN
    SELECT Id, TableNumber, Description, SeatingCapacity, TableLocation, Price, TableImagePath, Status, IsDeleted, CreatedAt, UpdatedAt
    FROM Tables
    WHERE Id = @Id AND IsDeleted = 0;
END;
