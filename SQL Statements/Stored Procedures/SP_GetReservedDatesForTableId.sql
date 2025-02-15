CREATE PROCEDURE GetReservedDatesForTableId 
    @TableId INT,
	@BuffetType VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * 
    FROM Reservations
    WHERE TableId = @TableId 
	AND BuffetType = @BuffetType
	AND Status IN ('Pending', 'Ongoing')
END;
