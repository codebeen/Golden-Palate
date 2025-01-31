CREATE PROCEDURE GetAvailableTables
AS
BEGIN
    SELECT *
    FROM Tables
    WHERE IsDeleted = 0 AND Status = 'Available'
END;
