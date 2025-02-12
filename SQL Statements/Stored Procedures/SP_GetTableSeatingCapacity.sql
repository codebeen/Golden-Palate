CREATE PROCEDURE GetTableSeatingCapacity
    @TableId INT
AS
BEGIN
    -- Select the seating capacity based on the given TableId
    SELECT *
    FROM Tables
    WHERE Id = @TableId;
END;
