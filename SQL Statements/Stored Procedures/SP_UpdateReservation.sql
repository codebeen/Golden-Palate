CREATE PROCEDURE UpdateReservation
    @Id INT,
    @ReservationDate DATE
AS
BEGIN
    UPDATE Reservations
    SET ReservationDate = @ReservationDate,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;
