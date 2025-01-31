CREATE PROCEDURE GetAllNotCancelledReservations
AS
BEGIN
    SELECT *
    FROM Reservations
	WHERE Status != 'Cancelled'
END;
