CREATE PROCEDURE GetAllTables
AS
BEGIN
    SELECT Id, TableNumber, Description, SeatingCapacity, TableLocation, Price, TableImagePath, Status, IsDeleted, CreatedAt, UpdatedAt
    FROM Tables
    WHERE IsDeleted = 0
	ORDER BY
		CreatedAt DESC;
END;
