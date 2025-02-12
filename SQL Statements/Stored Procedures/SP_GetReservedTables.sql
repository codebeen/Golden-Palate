CREATE PROCEDURE GetReservedTables 
    @ReservationDate DATE,
	@BuffetType VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * 
    FROM Reservations
    WHERE ReservationDate = @ReservationDate 
	AND BuffetType = @BuffetType
	AND Status IN ('Pending', 'Ongoing')
END;
