CREATE PROCEDURE CancelReservation
    @ReservationNumber VARCHAR(255)
AS
BEGIN
    UPDATE Reservations
    SET Status = 'Cancelled',
        UpdatedAt = GETDATE()
    WHERE ReservationNumber = @ReservationNumber;
END;
