CREATE PROCEDURE GetTableIdByTableNumber
    @TableNumber INT
AS
BEGIN
    -- Select the TableId for the given TableNumber
    SELECT Id
    FROM Tables
    WHERE TableNumber = @TableNumber;
END;
