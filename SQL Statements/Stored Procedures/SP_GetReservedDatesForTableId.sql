CREATE PROCEDURE GetReservedDatesForTableId 
    @Id INT,
	@BuffetType VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * 
    FROM Reservations
    WHERE TableId = @Id 
	AND BuffetType = @BuffetType
	AND Status IN ('Pending', 'Ongoing')
END;
