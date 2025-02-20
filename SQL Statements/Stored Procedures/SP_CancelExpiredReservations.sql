CREATE PROCEDURE CancelExpiredReservations
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Reservations
    SET Status = 'Cancelled'
    WHERE ReservationDate < CAST(GETDATE() AS DATE)
    AND Status = 'Pending';
END;
