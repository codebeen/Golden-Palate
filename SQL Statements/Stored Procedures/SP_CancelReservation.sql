CREATE PROCEDURE CancelReservation
    @Id INT
AS
BEGIN
    UPDATE Reservations
    SET Status = 'Cancelled',
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;
